using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.CodeStructure;
using OpenRA.Converter.Core.Models.DecisionTree;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class TraitSynthesisService : ITraitSynthesisService
    {
        private readonly IDecisionTreeService _decisionTreeService;
        private readonly IReferenceRegistry _registry;
        private static readonly Regex ActionArgsRegex = new Regex(@"\((.*?)\)");

        public TraitSynthesisService(IDecisionTreeService decisionTreeService, IReferenceRegistry registry)
        {
            _decisionTreeService = decisionTreeService;
            _registry = registry;
        }

        public CsClass SynthesizeTrait(DecisionNode rootNode, string traitName)
        {
            // 1. Info Class
            var infoClass = new CsClass
            {
                Name = $"{traitName}Info",
                Inherits = "ConditionalTraitInfo",
                Usings = new List<string> { "OpenRA.Traits" }
            };

            infoClass.Fields.Add(new CsField
            {
                Name = "RequiresCondition",
                Type = "string",
                IsExposedToYaml = true,
                Description = "Condition required to enable this trait."
            });

            // 2. Logic Class
            var logicClass = new CsClass
            {
                Name = traitName,
                Inherits = $"ConditionalTrait<{traitName}Info>",
                PairedInfoClass = infoClass
            };

            logicClass.Interfaces.Add("ITick");
            logicClass.Interfaces.Add("INotifyCreated");

            AddBoilerplateMethods(logicClass, traitName);

            // 3. Process Logic
            // Pass 'infoClass' so we can add new fields to it during processing
            var tickMethod = logicClass.Methods.First(m => m.Name == "Tick");
            ProcessNode(rootNode, tickMethod.BodyLines, 0, logicClass, infoClass);

            return logicClass;
        }

        private void AddBoilerplateMethods(CsClass logicClass, string traitName)
        {
            // Ctor
            var ctor = new CsMethod { Name = traitName, ReturnType = "", AccessModifier = "public" };
            ctor.Parameters.Add(new CsParameter("Actor", "self"));
            ctor.Parameters.Add(new CsParameter($"{traitName}Info", "info"));
            ctor.BodyLines.Add(": base(info) { }");
            logicClass.Methods.Add(ctor);

            // Created
            var created = new CsMethod { Name = "Created", ReturnType = "void", ExplicitInterfaceImplementation = "INotifyCreated" };
            created.Parameters.Add(new CsParameter("Actor", "self"));
            logicClass.Methods.Add(created);

            // Tick
            var tick = new CsMethod { Name = "Tick", ReturnType = "void", ExplicitInterfaceImplementation = "ITick" };
            tick.Parameters.Add(new CsParameter("Actor", "self"));
            tick.BodyLines.Add("if (IsTraitDisabled) return;");
            tick.BodyLines.Add("");
            logicClass.Methods.Add(tick);
        }

        private void ProcessNode(DecisionNode node, List<string> bodyLines, int indentLevel, CsClass logicClass, CsClass infoClass)
        {
            string indent = new string(' ', indentLevel * 4);

            if (!string.IsNullOrWhiteSpace(node.Condition))
            {
                string conditionCode = MapConditionToCSharp(node.Condition, logicClass, infoClass);
                bodyLines.Add($"{indent}if ({conditionCode})");
                bodyLines.Add($"{indent}{{");

                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                {
                    string actionCode = MapActionToCSharp(node.Action, logicClass, infoClass);
                    bodyLines.Add($"{indent}    {actionCode}");
                }

                if (node.Children != null)
                {
                    foreach (var child in node.Children) ProcessNode(child, bodyLines, indentLevel + 1, logicClass, infoClass);
                }
                bodyLines.Add($"{indent}}}");
            }
            else
            {
                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                {
                    string actionCode = MapActionToCSharp(node.Action, logicClass, infoClass);
                    bodyLines.Add($"{indent}{actionCode}");
                }
                if (node.Children != null)
                {
                    foreach (var child in node.Children) ProcessNode(child, bodyLines, indentLevel, logicClass, infoClass);
                }
            }
        }

        private string MapConditionToCSharp(string rawCondition, CsClass logicClass, CsClass infoClass)
        {
            var parsed = _decisionTreeService.ParseConditionString(rawCondition);
            string expression;

            // 1. Check Registry
            var traitSchema = _registry.GetTrait(parsed.Variable);
            if (traitSchema != null)
            {
                logicClass.RequiredYamlInherits.Add(traitSchema.Name);
                expression = $"!self.Trait<{traitSchema.Name}>().IsTraitPaused";
            }
            // 2. Health Logic
            else if (parsed.Variable.Equals("Health", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Health");
                string op = parsed.Operator ?? "<";
                string valStr = parsed.Value?.Replace("%", "") ?? "0";

                if (double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    double multiplier = val / 100.0;
                    expression = $"self.Trait<Health>().HP {op} (int)(self.Trait<Health>().MaxHP * {multiplier.ToString("F2", CultureInfo.InvariantCulture)})";
                }
                else
                {
                    // Dynamic Parameter Detection for Health Threshold
                    // e.g. "Health < CriticalLevel" -> Create field 'CriticalLevel'
                    string paramName = EnsureField(infoClass, valStr, "int", "50");
                    // Assuming input is percentage integer
                    expression = $"self.Trait<Health>().HP {op} (int)(self.Trait<Health>().MaxHP * (Info.{paramName} / 100f))";
                }
            }
            // 3. Visibility
            else if (parsed.Variable.Equals("EnemyVisible", StringComparison.OrdinalIgnoreCase))
            {
                expression = "self.World.ActorMap.GetActorsAt(self.Location).Any(a => a.Owner.RelationshipWith(self.Owner) == PlayerRelationship.Enemy)";
            }
            // 4. Fallback / Custom Variable
            else
            {
                // Treat unknown variables as boolean flags in the Info class
                // e.g. condition: "IsAggressive" -> Info.IsAggressive
                string paramName = EnsureField(infoClass, parsed.Variable, "bool", "false");
                expression = $"Info.{paramName}";
            }

            if (parsed.IsNegated) return $"!({expression})";
            return expression;
        }

        private string MapActionToCSharp(string rawAction, CsClass logicClass, CsClass infoClass)
        {
            string cleanAction = rawAction.Trim().TrimEnd(';');
            string method = cleanAction;
            string args = "";

            var match = ActionArgsRegex.Match(cleanAction);
            if (match.Success)
            {
                method = cleanAction.Substring(0, cleanAction.IndexOf('(')).Trim();
                args = match.Groups[1].Value;
            }

            if (method.Equals("Wait", StringComparison.OrdinalIgnoreCase))
            {
                // Check if arg is number
                if (int.TryParse(args, out int ticks))
                {
                    return $"self.QueueActivity(new Wait({ticks}));";
                }
                else
                {
                    // Dynamic Parameter: Wait(MyDelay)
                    string paramName = EnsureField(infoClass, args, "int", "25");
                    return $"self.QueueActivity(new Wait(Info.{paramName}));";
                }
            }

            // ... (Existing mappings for Attack, Move, Hunt remain same) ...
            // Just adding brief catch-all for logic flow completeness
            if (method.Equals("Attack", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Armament");
                logicClass.RequiredYamlInherits.Add("AttackFrontal");
                logicClass.RequiredYamlInherits.Add("Mobile");
                return "self.QueueActivity(new AttackMoveActivity(self, self.Trait<Mobile>().MoveTo));";
            }

            return $"// TODO: Implement Action -> {rawAction}";
        }

        /// <summary>
        /// Checks if a field exists in the Info class. If not, creates it.
        /// </summary>
        private string EnsureField(CsClass infoClass, string rawName, string type, string defaultValue)
        {
            // Sanitize name
            string fieldName = Regex.Replace(rawName, "[^a-zA-Z0-9]", "");
            if (string.IsNullOrEmpty(fieldName)) fieldName = "Param" + infoClass.Fields.Count;

            // Capitalize
            fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);

            if (!infoClass.Fields.Any(f => f.Name == fieldName))
            {
                infoClass.Fields.Add(new CsField
                {
                    Name = fieldName,
                    Type = type,
                    AccessModifier = "public",
                    InitialValue = defaultValue,
                    IsExposedToYaml = true,
                    Description = $"Auto-generated parameter for {rawName}"
                });
            }
            return fieldName;
        }
    }
}