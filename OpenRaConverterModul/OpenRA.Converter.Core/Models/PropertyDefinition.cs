using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Converter.Core.Models
{
    /// <summary>
    /// Defines a single configurable field within a Trait or Weapon.
    /// </summary>
    public class PropertyDefinition
    {
        /// <summary>
        /// The name of the property (e.g., "Speed").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The data type expected by the OpenRA engine (e.g., "Integer", "Boolean", "1D World Distance").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The default value if not specified in YAML.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Documentation describing the property.
        /// </summary>
        public string? Description { get; set; }
    }
}
