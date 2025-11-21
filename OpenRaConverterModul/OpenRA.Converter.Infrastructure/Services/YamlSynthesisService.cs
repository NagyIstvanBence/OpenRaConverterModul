using System.Collections.Generic;
using System.Linq;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.CodeStructure;
using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class YamlSynthesisService : IYamlSynthesisService
    {
        public YamlNode SynthesizeActor(DecisionNode rootNode, CsClass generatedTrait, string actorName)
        {
            var actorNode = new YamlNode(actorName.ToUpperInvariant());

            // 1. Basic Inheritance
            actorNode.Children.Add(new YamlNode("Inherits", "^Soldier"));

            // 2. Auto-detected Dependencies from C# Analysis
            // (e.g. if code used Mobile, we verify/add it here, or add specific config overrides)
            if (generatedTrait.RequiredYamlInherits.Count > 0)
            {
                // In OpenRA, you usually inherit the whole ^Soldier, but if we needed specific traits
                // we might add them here. For now, we just add a comment listing them.
                var deps = string.Join(", ", generatedTrait.RequiredYamlInherits);
                actorNode.Children.Add(new YamlNode("Comment", $"Detected Dependencies: {deps}") { Comment = "Logic requirements" });
            }

            // 3. The Custom Trait
            var customTraitNode = new YamlNode(generatedTrait.Name);

            // 4. Dynamic Parameters
            // Iterate over the Info class fields we just generated and add them to YAML
            if (generatedTrait.PairedInfoClass != null)
            {
                foreach (var field in generatedTrait.PairedInfoClass.Fields.Where(f => f.IsExposedToYaml))
                {
                    // Use initial value as default
                    string val = field.InitialValue?.Replace("\"", "") ?? "0";
                    customTraitNode.Children.Add(new YamlNode(field.Name, val));
                }
            }

            actorNode.Children.Add(customTraitNode);

            return actorNode;
        }
    }
}