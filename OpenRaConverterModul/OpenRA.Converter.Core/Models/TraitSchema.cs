using System.Collections.Generic;

namespace OpenRA.Converter.Core.Models
{
    /// <summary>
    /// Represents the normalized definition of a specific OpenRA Trait (e.g., "Mobile", "Health").
    /// </summary>
    public class TraitSchema
    {
        /// <summary>
        /// The unique name of the trait (e.g., "AttackFrontal").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// A dictionary of valid properties for this trait (e.g., "Speed" -> "Integer").
        /// </summary>
        public Dictionary<string, PropertyDefinition> Properties { get; set; } = new();

        /// <summary>
        /// A list of base traits this trait inherits logic from (e.g., "ConditionalTrait").
        /// </summary>
        public List<string> InheritedTraits { get; set; } = new();

        /// <summary>
        /// A list of other traits required on the actor for this trait to function (e.g., "Mobile").
        /// </summary>
        public List<string> RequiredTraits { get; set; } = new();
    }
}