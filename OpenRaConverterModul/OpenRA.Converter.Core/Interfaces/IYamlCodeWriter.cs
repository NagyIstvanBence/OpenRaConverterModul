using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Core.Interfaces
{
    public interface IYamlCodeWriter
    {
        /// <summary>
        /// Converts the YAML AST into a formatted string.
        /// </summary>
        string WriteYaml(YamlNode rootNode);
    }
}