using System.Text;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class YamlCodeWriter : IYamlCodeWriter
    {
        public string WriteYaml(YamlNode rootNode)
        {
            var sb = new StringBuilder();
            WriteNode(sb, rootNode, 0);
            return sb.ToString();
        }

        private void WriteNode(StringBuilder sb, YamlNode node, int indentLevel)
        {
            // OpenRA uses Tabs for indentation, but spaces are safer for general YAML.
            // We will use 1 Tab (\t) per level as per OpenRA standard.
            var indent = new string('\t', indentLevel);

            sb.Append(indent);
            sb.Append(node.Key);
            sb.Append(":");

            if (!string.IsNullOrEmpty(node.Value))
            {
                sb.Append(" ");
                sb.Append(node.Value);
            }

            if (!string.IsNullOrEmpty(node.Comment))
            {
                sb.Append(" # ");
                sb.Append(node.Comment);
            }

            sb.AppendLine();

            foreach (var child in node.Children)
            {
                WriteNode(sb, child, indentLevel + 1);
            }
        }
    }
}