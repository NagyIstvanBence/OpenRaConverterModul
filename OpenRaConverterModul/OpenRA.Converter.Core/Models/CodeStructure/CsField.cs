namespace OpenRA.Converter.Core.Models.CodeStructure
{
    /// <summary>
    /// Represents a field variable in a C# class (e.g., "public int Delay = 5;").
    /// </summary>
    public class CsField
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "int"; // Default to int
        public string AccessModifier { get; set; } = "public"; // public, private, protected
        public string? InitialValue { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsStatic { get; set; }

        /// <summary>
        /// If true, this field corresponds to a parameter in the *Info class 
        /// and should be loaded from YAML.
        /// </summary>
        public bool IsExposedToYaml { get; set; }
        public string Description { get; set; }
    }
}