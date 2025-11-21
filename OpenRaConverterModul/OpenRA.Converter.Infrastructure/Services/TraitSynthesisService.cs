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

        // Helper regex to extract arguments from actions like "Move(destination)"
        private static readonly Regex ActionArgsRegex = new Regex(@"\((.*?)\)");

        public TraitSynthesisService(IDecisionTreeService decisionTreeService, IReferenceRegistry registry)
        {
            _decisionTreeService = decisionTreeService;
            _registry = registry;
        }

        public CsClass SynthesizeTrait(DecisionNode rootNode, string traitName)
        {
            // 1. Create Configuration Class
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

            // 2. Create Logic Class
            var logicClass = new CsClass
            {
                Name = traitName,
                Inherits = $"ConditionalTrait<{traitName}Info>",
                PairedInfoClass = infoClass
            };

            logicClass.Interfaces.Add("ITick");
            logicClass.Interfaces.Add("INotifyCreated");

            // 3. Generate Boilerplate Methods (Constructor, Created, Tick)
            AddBoilerplateMethods(logicClass, traitName);

            // 4. Recursively build Logic & Resolve Dependencies
            // We pass the logicClass to track dependencies found during tree traversal
            ProcessNode(rootNode, logicClass.Methods.First(m => m.Name == "Tick").BodyLines, 0, logicClass);

            return logicClass;
        }

        private void AddBoilerplateMethods(CsClass logicClass, string traitName)
        {
            // Constructor
            var ctor = new CsMethod
            {
                Name = traitName,
                AccessModifier = "public",
                ReturnType = "",
                Parameters = new List<CsParameter>
                {
                    new CsParameter("Actor", "self"),
                    new CsParameter($"{traitName}Info", "info")
                }
            };
            ctor.BodyLines.Add(": base(info) { }");
            logicClass.Methods.Add(ctor);

            // Created
            var created = new CsMethod
            {
                Name = "Created",
                ReturnType = "void",
                ExplicitInterfaceImplementation = "INotifyCreated",
                Parameters = new List<CsParameter> { new CsParameter("Actor", "self") }
            };
            created.BodyLines.Add("// TODO: Cache traits here");
            logicClass.Methods.Add(created);

            // Tick
            var tick = new CsMethod
            {
                Name = "Tick",
                ReturnType = "void",
                ExplicitInterfaceImplementation = "ITick",
                Parameters = new List<CsParameter> { new CsParameter("Actor", "self") }
            };
            tick.BodyLines.Add("if (IsTraitDisabled) return;");
            tick.BodyLines.Add("");
            logicClass.Methods.Add(tick);
        }

        private void ProcessNode(DecisionNode node, List<string> bodyLines, int indentLevel, CsClass logicClass)
        {
            string indent = new string(' ', indentLevel * 4);

            // Case 1: Branch (Condition)
            if (!string.IsNullOrWhiteSpace(node.Condition))
            {
                string cSharpCondition = MapConditionToCSharp(node.Condition, logicClass);
                bodyLines.Add($"{indent}if ({cSharpCondition})");
                bodyLines.Add($"{indent}{{");

                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                {
                    string actionCode = MapActionToCSharp(node.Action, logicClass);
                    bodyLines.Add($"{indent}    {actionCode}");
                }

                if (node.Children != null)
                {
                    foreach (var child in node.Children) ProcessNode(child, bodyLines, indentLevel + 1, logicClass);
                }

                bodyLines.Add($"{indent}}}");
            }
            // Case 2: Unconditional
            else
            {
                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                {
                    string actionCode = MapActionToCSharp(node.Action, logicClass);
                    bodyLines.Add($"{indent}{actionCode}");
                }

                if (node.Children != null)
                {
                    foreach (var child in node.Children) ProcessNode(child, bodyLines, indentLevel, logicClass);
                }
            }
        }

        private string MapConditionToCSharp(string rawCondition, CsClass logicClass)
        {
            var parsed = _decisionTreeService.ParseConditionString(rawCondition);
            string expression;

            // 1. Check against Registry: Does this variable match a known Trait Name?
            // e.g. "Mobile" -> Check if actor has Mobile trait
            var traitSchema = _registry.GetTrait(parsed.Variable);
            if (traitSchema != null)
            {
                // Auto-dependency: If checking "Mobile", we need "Mobile" trait.
                logicClass.RequiredYamlInherits.Add(traitSchema.Name);
                expression = $"!self.Trait<{traitSchema.Name}>().IsTraitPaused";
            }
            // 2. Health Check
            else if (parsed.Variable.Equals("Health", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Health"); // Dependency
                string op = parsed.Operator ?? "<";
                string valStr = parsed.Value?.Replace("%", "") ?? "0";

                if (double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    double multiplier = val / 100.0;
                    expression = $"self.Trait<Health>().HP {op} (int)(self.Trait<Health>().MaxHP * {multiplier.ToString("F2", CultureInfo.InvariantCulture)})";
                }
                else
                {
                    expression = "false /* Error: Invalid Health Value */";
                }
            }
            // 3. Visibility
            else if (parsed.Variable.Equals("EnemyVisible", StringComparison.OrdinalIgnoreCase))
            {
                expression = "self.World.ActorMap.GetActorsAt(self.Location).Any(a => a.Owner.RelationshipWith(self.Owner) == PlayerRelationship.Enemy)";
            }
            // 4. Generic Fallback
            else
            {
                expression = parsed.Variable; // Assume local variable or helper
            }

            if (parsed.IsNegated) return $"!({expression})";
            return expression;
        }

        private string MapActionToCSharp(string rawAction, CsClass logicClass)
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

            // --- Action Mapping with Dependency Injection ---

            if (method.Equals("Attack", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Armament");
                logicClass.RequiredYamlInherits.Add("AttackFrontal");
                return "self.QueueActivity(new AttackMoveActivity(self, self.Trait<Mobile>().MoveTo));";
            }

            if (method.Equals("Move", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Mobile");
                return "self.QueueActivity(new Move(self, self.Location)); // TODO: Resolve Destination";
            }

            if (method.Equals("Wait", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(args, out int ticks)) return $"self.QueueActivity(new Wait({ticks}));";
                return "self.QueueActivity(new Wait(25));";
            }

            if (method.Equals("Hunt", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("AttackMove");
                return "self.QueueActivity(new Hunt(self));";
            }

            return $"// TODO: Implement Action -> {rawAction}";
        }
    }
}