using System.Collections.Generic;

namespace OpenRA.Converter.Core.Models.CodeStructure
{
    /// <summary>
    /// Represents a full C# class file structure (Imports, Namespace, Class definition).
    /// </summary>
    public class CsClass
    {
        public string Namespace { get; set; } = "OpenRA.Mods.Common.Traits";
        public List<string> Usings { get; set; } = new()
        {
            "System",
            "System.Collections.Generic",
            "OpenRA.Traits",
            "OpenRA.Mods.Common.Traits"
        };

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Base class (e.g., "ConditionalTrait<MyInfo>").
        /// </summary>
        public string Inherits { get; set; } = "TraitInfo";

        /// <summary>
        /// Interfaces implemented (e.g., "ITick", "INotifyCreated").
        /// </summary>
        public List<string> Interfaces { get; set; } = new();

        public List<CsField> Fields { get; set; } = new();
        public List<CsMethod> Methods { get; set; } = new();

        // For OpenRA, we typically generate pairs: An Info class and a Logic class.
        // This property can hold the reference to the "Paired" class if this is the Logic class.
        public CsClass? PairedInfoClass { get; set; }
    }
}