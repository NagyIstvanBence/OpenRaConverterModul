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
            "System.Linq",
            "OpenRA.Traits",
            "OpenRA.Mods.Common.Traits",
            "OpenRA.Mods.Common.Activities"
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

        // Reference to the paired class (Logic -> Info)
        public CsClass? PairedInfoClass { get; set; }

        /// <summary>
        /// List of OpenRA traits (YAML) required for this C# class to function.
        /// E.g., "Mobile", "AttackFrontal".
        /// </summary>
        public HashSet<string> RequiredYamlInherits { get; set; } = new();
    }
}