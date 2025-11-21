using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.DecisionTree;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class DecisionTreeService : IDecisionTreeService
    {
        private readonly IReferenceRegistry _registry;

        // Regex to identify operators: <, >, <=, >=, ==, !=
        // Matches groups: (Variable) (Operator) (Value)
        private static readonly Regex ConditionRegex = new Regex(@"^\s*(?:Not\s+)?([a-zA-Z0-9_]+)\s*([<>=!]+)\s*(.+)\s*$", RegexOptions.IgnoreCase);

        public DecisionTreeService(IReferenceRegistry registry)
        {
            _registry = registry;
        }

        public DecisionNode ParseTree(JsonElement rootElement)
        {
            try
            {
                var node = JsonSerializer.Deserialize<DecisionNode>(rootElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (node == null) throw new ArgumentException("Failed to deserialize decision tree.");

                return node;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException($"Invalid JSON format: {ex.Message}");
            }
        }

        public List<string> ValidateTree(DecisionNode root)
        {
            var errors = new List<string>();
            TraverseAndValidate(root, errors);
            return errors;
        }

        private void TraverseAndValidate(DecisionNode node, List<string> errors)
        {
            // 1. Structural Validation
            if (string.IsNullOrWhiteSpace(node.Id))
                errors.Add("Node found without an ID.");

            if (node.IsLeaf && node.Children != null && node.Children.Count > 0)
                errors.Add($"Node '{node.Id}' is a Leaf (has Action) but also contains Children. This is ambiguous.");

            if (!node.IsLeaf && (node.Children == null || node.Children.Count == 0) && !node.IsRoot)
                // Note: Root might be empty if it's a WIP, but generally branches need children.
                // We'll allow it but maybe warn.
                errors.Add($"Branch Node '{node.Id}' has no children and no action. It is a dead end.");

            // 2. Logic Validation
            if (!string.IsNullOrWhiteSpace(node.Condition))
            {
                try
                {
                    var parsed = ParseConditionString(node.Condition);
                    // Optional: Validate against Registry here
                    // var trait = _registry.GetTrait(parsed.Variable);
                    // if (trait == null) errors.Add($"Node '{node.Id}' references unknown trait/variable '{parsed.Variable}'");
                }
                catch (Exception)
                {
                    errors.Add($"Node '{node.Id}' has invalid condition syntax: '{node.Condition}'");
                }
            }

            // 3. Recurse
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    TraverseAndValidate(child, errors);
                }
            }
        }

        public ParsedCondition ParseConditionString(string rawCondition)
        {
            var result = new ParsedCondition();
            rawCondition = rawCondition.Trim();

            // Check for negation
            if (rawCondition.StartsWith("Not ", StringComparison.OrdinalIgnoreCase) || rawCondition.StartsWith("!"))
            {
                result.IsNegated = true;
                // Remove "Not " or "!" for parsing the rest
                rawCondition = rawCondition.StartsWith("!") ? rawCondition.Substring(1).Trim() : rawCondition.Substring(4).Trim();
            }

            var match = ConditionRegex.Match(rawCondition);
            if (match.Success)
            {
                // It's a comparison (e.g., Health < 30%)
                result.Variable = match.Groups[1].Value;
                result.Operator = match.Groups[2].Value;
                result.Value = match.Groups[3].Value;
            }
            else
            {
                // It's a simple boolean flag (e.g., "EnemyVisible")
                result.Variable = rawCondition;
                result.Operator = null;
                result.Value = null;
            }

            return result;
        }
    }
}