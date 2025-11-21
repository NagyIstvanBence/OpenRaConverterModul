namespace OpenRA.Converter.Core.Models.CodeStructure
{
    /// <summary>
    /// Represents a parameter in a method signature.
    /// </summary>
    public class CsParameter
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public CsParameter(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}