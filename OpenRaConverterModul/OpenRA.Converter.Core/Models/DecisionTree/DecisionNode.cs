using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenRA.Converter.Core.Models.DecisionTree
{
    /// <summary>
    /// Represents a single node in the behavior decision tree.
    /// Can be a Branch (Logic) or a Leaf (Action).
    /// </summary>
    public class DecisionNode
    {
        /// <summary>
        /// Unique identifier for the node (e.g., "low_health").
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The raw condition string (e.g., "Health < 30%", "HasRocket").
        /// Null if this is an unconditional entry point or action-only node.
        /// </summary>
        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        /// <summary>
        /// The raw action string (e.g., "Attack(self, target)").
        /// Only present on Leaf nodes.
        /// </summary>
        [JsonPropertyName("action")]
        public string? Action { get; set; }

        /// <summary>
        /// Child nodes to evaluate if this node's condition is met.
        /// </summary>
        [JsonPropertyName("children")]
        public List<DecisionNode>? Children { get; set; }

        // --- Helper Properties for internal processing ---

        [JsonIgnore]
        public bool IsLeaf => !string.IsNullOrWhiteSpace(Action);

        [JsonIgnore]
        public bool IsRoot => Id.Equals("root", System.StringComparison.OrdinalIgnoreCase);
    }
}