namespace OpenRA.Converter.Core.Models.DecisionTree
{
    /// <summary>
    /// Represents a parsed version of a logic string like "Health < 30%".
    /// </summary>
    public class ParsedCondition
    {
        /// <summary>
        /// The subject of the condition (e.g., "Health", "EnemyVisible").
        /// </summary>
        public string Variable { get; set; } = string.Empty;

        /// <summary>
        /// The operator (e.g., "<", ">", "==", "!"). 
        /// Null if it's a simple boolean flag.
        /// </summary>
        public string? Operator { get; set; }

        /// <summary>
        /// The comparison value (e.g., "30%").
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Whether this condition is negated (e.g., "Not CanFlank").
        /// </summary>
        public bool IsNegated { get; set; }
    }
}