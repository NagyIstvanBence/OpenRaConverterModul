using System.Text.Json;
using OpenRA.Converter.Core.Models.DecisionTree;

namespace OpenRA.Converter.Core.Interfaces
{
    public interface IDecisionTreeService
    {
        /// <summary>
        /// Parses a raw JSON tree into a typed object graph.
        /// </summary>
        DecisionNode ParseTree(JsonElement rootElement);

        /// <summary>
        /// Validates the tree structure (e.g., checking for dead ends or invalid logic syntax).
        /// </summary>
        /// <param name="root">The root of the parsed tree.</param>
        /// <returns>A list of warnings or errors. Empty if valid.</returns>
        List<string> ValidateTree(DecisionNode root);

        /// <summary>
        /// Parses a raw condition string into a structured object.
        /// Example: "Health < 30%" -> { Variable: "Health", Operator: "<", Value: "0.3" }
        /// </summary>
        ParsedCondition ParseConditionString(string rawCondition);
    }
}