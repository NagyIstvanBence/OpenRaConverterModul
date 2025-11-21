using System.Collections.Generic;

namespace OpenRA.Converter.Core.Models
{
    /// <summary>
    /// Represents the normalized definition of a specific OpenRA Weapon Type (e.g., "Missile", "Bullet").
    /// </summary>
    public class WeaponSchema
    {
        /// <summary>
        /// The unique type name of the weapon (e.g., "Missile").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// A description of what this weapon type does.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// A dictionary of valid properties for this weapon type (e.g., "Range" -> "1D World Distance").
        /// </summary>
        public Dictionary<string, PropertyDefinition> Properties { get; set; } = new();
    }
}