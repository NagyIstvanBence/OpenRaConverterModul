using System.Collections.Generic;

namespace OpenRA.Converter.Core.Models.YamlStructure
{
    /// <summary>
    /// Represents a node in the OpenRA MiniYaml tree.
    /// Format: "Key: Value" or nested children.
    /// </summary>
    public class YamlNode
    {
        public string Key { get; set; } = string.Empty;
        public string? Value { get; set; }
        public List<YamlNode> Children { get; set; } = new();
        public string? Comment { get; set; }

        public YamlNode(string key, string? value = null)
        {
            Key = key;
            Value = value;
        }
    }
}