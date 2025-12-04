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
        // Regex a koordináták (12,34) felismerésére
        private static readonly Regex PointRegex = new Regex(@"\(?(\d+),\s*(\d+)\)?");

        public TraitSynthesisService(IDecisionTreeService decisionTreeService, IReferenceRegistry registry)
        {
            _decisionTreeService = decisionTreeService;
            _registry = registry;
        }

        public CsClass SynthesizeTrait(DecisionNode rootNode, string traitName)
        {
            var infoClass = new CsClass
            {
                Name = $"{traitName}Info",
                Inherits = "ConditionalTraitInfo",
                Usings = new List<string> { "OpenRA.Traits", "OpenRA.Mods.Common.Traits", "OpenRA.Mods.Common.Activities", "OpenRA.Primitives" }
            };

            infoClass.Fields.Add(new CsField
            {
                Name = "Period",
                Type = "int",
                IsExposedToYaml = true,
                InitialValue = "25",
                Description = "Time in ticks to wait between checks."
            });

            var createMethod = new CsMethod
            {
                Name = "Create",
                ReturnType = "object",
                AccessModifier = "public override"
            };
            createMethod.Parameters.Add(new CsParameter("ActorInitializer", "init"));
            createMethod.BodyLines.Add($"return new {traitName}(init.Self, this);");
            infoClass.Methods.Add(createMethod);

            var logicClass = new CsClass
            {
                Name = traitName,
                Inherits = $"ConditionalTrait<{traitName}Info>",
                PairedInfoClass = infoClass
            };

            logicClass.Interfaces.Add("ITick");
            logicClass.Interfaces.Add("INotifyCreated");

            AddBoilerplateMethods(logicClass, traitName);

            var tickMethod = logicClass.Methods.First(m => m.Name == "Tick");
            ProcessNode(rootNode, tickMethod.BodyLines, 0, logicClass, infoClass);

            return logicClass;
        }

        private void AddBoilerplateMethods(CsClass logicClass, string traitName)
        {
            var ctor = new CsMethod { Name = traitName, ReturnType = "", AccessModifier = "public" };
            ctor.Parameters.Add(new CsParameter("Actor", "self"));
            ctor.Parameters.Add(new CsParameter($"{traitName}Info", "info"));
            ctor.BodyLines.Add(": base(info)");
            ctor.BodyLines.Add("_ticksRemaining = info.Period;");
            logicClass.Methods.Add(ctor);

            logicClass.Fields.Add(new CsField { Name = "_ticksRemaining", Type = "int", AccessModifier = "private" });

            // JAVÍTÁS: Explicit interface implementációnál az AccessModifier üres
            var created = new CsMethod { Name = "Created", ReturnType = "void", ExplicitInterfaceImplementation = "INotifyCreated", AccessModifier = "" };
            created.Parameters.Add(new CsParameter("Actor", "self"));
            logicClass.Methods.Add(created);

            // JAVÍTÁS: Explicit interface implementációnál az AccessModifier üres
            var tick = new CsMethod { Name = "Tick", ReturnType = "void", ExplicitInterfaceImplementation = "ITick", AccessModifier = "" };
            tick.Parameters.Add(new CsParameter("Actor", "self"));
            tick.BodyLines.Add("if (IsTraitDisabled) return;");
            tick.BodyLines.Add("if (self.CurrentActivity != null) return;");
            tick.BodyLines.Add("if (--_ticksRemaining > 0) return;");
            tick.BodyLines.Add("_ticksRemaining = Info.Period;");
            tick.BodyLines.Add("");

            logicClass.Methods.Add(tick);
        }

        private void ProcessNode(DecisionNode node, List<string> bodyLines, int indentLevel, CsClass logicClass, CsClass infoClass, bool skipConditionWrapper = false)
        {
            string indent = new string('\t', indentLevel);
            var trueChildren = new List<DecisionNode>();
            var falseChildren = new List<DecisionNode>();

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (!string.IsNullOrWhiteSpace(node.Condition) && IsNegation(node.Condition, child.Condition))
                        falseChildren.Add(child);
                    else
                        trueChildren.Add(child);
                }
            }

            if (!string.IsNullOrWhiteSpace(node.Condition) && !skipConditionWrapper)
            {
                string conditionCode = MapConditionToCSharp(node.Condition, logicClass, infoClass);
                bodyLines.Add($"{indent}if ({conditionCode})");
                bodyLines.Add($"{indent}{{");

                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                    bodyLines.Add($"{indent}\t{MapActionToCSharp(node.Action, logicClass, infoClass)}");

                ProcessChildList(trueChildren, bodyLines, indentLevel + 1, logicClass, infoClass);

                bodyLines.Add($"{indent}}}");

                if (falseChildren.Any())
                {
                    bodyLines.Add($"{indent}else");
                    bodyLines.Add($"{indent}{{");
                    ProcessChildList(falseChildren, bodyLines, indentLevel + 1, logicClass, infoClass, stripCondition: true);
                    bodyLines.Add($"{indent}}}");
                }
            }
            else
            {
                if (node.IsLeaf && !string.IsNullOrWhiteSpace(node.Action))
                    bodyLines.Add($"{indent}{MapActionToCSharp(node.Action, logicClass, infoClass)}");

                ProcessChildList(node.Children, bodyLines, indentLevel, logicClass, infoClass);
            }
        }

        private void ProcessChildList(List<DecisionNode> nodes, List<string> bodyLines, int indentLevel, CsClass logicClass, CsClass infoClass, bool stripCondition = false)
        {
            if (nodes == null) return;
            string indent = new string('\t', indentLevel);
            var processed = new HashSet<DecisionNode>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var currentNode = nodes[i];
                if (processed.Contains(currentNode)) continue;

                DecisionNode negationNode = null;
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    if (!processed.Contains(nodes[j]) && IsNegation(currentNode.Condition, nodes[j].Condition))
                    {
                        negationNode = nodes[j];
                        break;
                    }
                }

                if (negationNode != null)
                {
                    ProcessNode(currentNode, bodyLines, indentLevel, logicClass, infoClass, skipConditionWrapper: false);
                    processed.Add(currentNode);
                    processed.Add(negationNode);

                    bodyLines.Add($"{indent}else");
                    bodyLines.Add($"{indent}{{");
                    ProcessNode(negationNode, bodyLines, indentLevel + 1, logicClass, infoClass, skipConditionWrapper: true);
                    bodyLines.Add($"{indent}}}");
                }
                else
                {
                    ProcessNode(currentNode, bodyLines, indentLevel, logicClass, infoClass, skipConditionWrapper: stripCondition);
                    processed.Add(currentNode);
                }
            }
        }

        private bool IsNegation(string conditionA, string conditionB)
        {
            if (string.IsNullOrEmpty(conditionA) || string.IsNullOrEmpty(conditionB)) return false;
            var a = conditionA.Trim();
            var b = conditionB.Trim();
            if (a.Equals($"Not {b}", StringComparison.OrdinalIgnoreCase) || b.Equals($"Not {a}", StringComparison.OrdinalIgnoreCase)) return true;
            if (a.Equals($"!{b}", StringComparison.OrdinalIgnoreCase) || b.Equals($"!{a}", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private string MapConditionToCSharp(string rawCondition, CsClass logicClass, CsClass infoClass)
        {
            var parsed = _decisionTreeService.ParseConditionString(rawCondition);
            string expression;

            if (parsed.Variable.Equals("Health", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Health");
                string op = parsed.Operator ?? "<";
                string valStr = parsed.Value?.Replace("%", "") ?? "0";

                if (double.TryParse(valStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double val))
                {
                    // JAVÍTÁS: CultureInfo.InvariantCulture használata a string interpolációban
                    // Ez javítja a CS0030 hibát (0,3 -> 0.3)
                    expression = $"self.Trait<Health>().HP {op} (int)(self.Trait<Health>().MaxHP * {(val / 100.0).ToString(CultureInfo.InvariantCulture)})";
                }
                else
                {
                    string paramName = EnsureField(infoClass, valStr, "int", "50");
                    expression = $"self.Trait<Health>().HP {op} (int)(self.Trait<Health>().MaxHP * (Info.{paramName} / 100f))";
                }
            }
            else if (parsed.Variable.Equals("EnemyVisible", StringComparison.OrdinalIgnoreCase))
            {
                expression = "self.World.ActorMap.GetActorsAt(self.Location).Any(a => a.Owner.RelationshipWith(self.Owner) == PlayerRelationship.Enemy)";
            }
            else
            {
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

            var argList = args.Split(',').Select(a => a.Trim()).ToList();
            string firstArg = argList.FirstOrDefault() ?? "";

            if (method.Equals("Wait", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(firstArg, out int ticks)) return $"self.QueueActivity(new Wait({ticks}));";
                string paramName = EnsureField(infoClass, firstArg, "int", "25");
                return $"self.QueueActivity(new Wait(Info.{paramName}));";
            }

            // JAVÍTÁS: CS1503 hiba kezelése
            if (method.Contains("Attack", StringComparison.OrdinalIgnoreCase) || method.Contains("Hunt"))
            {
                logicClass.RequiredYamlInherits.Add("Armament");
                logicClass.RequiredYamlInherits.Add("AttackFrontal");
                logicClass.RequiredYamlInherits.Add("Mobile");

                // Az "AttackMoveActivity" csak célponttal működik jól.
                // Ha általános támadásról van szó, a "Hunt" a helyes activity OpenRA-ben.
                // Ez megszünteti a method group konverziós hibát.
                return "self.QueueActivity(new Hunt(self));";
            }

            if (method.Equals("Move", StringComparison.OrdinalIgnoreCase))
            {
                logicClass.RequiredYamlInherits.Add("Mobile");

                // JAVÍTÁS: Koordináták helyes generálása (x, y) -> new CPos(x, y)
                var pointMatch = PointRegex.Match(args);
                if (pointMatch.Success)
                {
                    string x = pointMatch.Groups[1].Value;
                    string y = pointMatch.Groups[2].Value;
                    return $"self.QueueActivity(new Move(self, new CPos({x}, {y})));";
                }

                return "self.QueueActivity(new Move(self, self.Location)); // Warning: Destination undefined";
            }

            if (!method.Equals(rawAction))
            {
                return $"// TODO: Implement Custom Action -> {rawAction}";
            }

            return $"// TODO: Implement Action -> {rawAction}";
        }

        private string EnsureField(CsClass infoClass, string rawName, string type, string defaultValue)
        {
            string fieldName = Regex.Replace(rawName, "[^a-zA-Z0-9]", "");
            if (string.IsNullOrEmpty(fieldName)) fieldName = "Param" + infoClass.Fields.Count;
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