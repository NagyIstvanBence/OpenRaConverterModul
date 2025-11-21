using System.Collections.Generic;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class YamlSynthesisService : IYamlSynthesisService
    {
        public YamlNode SynthesizeActor(DecisionNode rootNode, string actorName, string traitName)
        {
            // 1. Define the Root Actor Node
            var actorNode = new YamlNode(actorName.ToUpperInvariant());

            // 2. Add Standard Inheritance (Hardcoded for MVP, dynamic later)
            actorNode.Children.Add(new YamlNode("Inherits", "^Soldier"));

            // 3. Add Basic Description
            actorNode.Children.Add(new YamlNode("Tooltip")
            {
                Children = new List<YamlNode>
                {
                    new YamlNode("Name", $"{actorName} (AI Controlled)"),
                    new YamlNode("GenericName", "Infantry")
                }
            });

            // 4. Add Render Logic (Placeholder)
            var renderNode = new YamlNode("RenderSprites");
            renderNode.Children.Add(new YamlNode("Image", "e1")); // Default to standard rifleman sprite
            actorNode.Children.Add(renderNode);

            // 5. Attach the Custom Generated Trait
            // The Key is the class name of the trait we generated in C#
            var customTraitNode = new YamlNode(traitName);

            // 5.1 Add default configuration for the trait
            // In Phase 6, we will scan the DecisionNode to populate these values dynamically.
            customTraitNode.Children.Add(new YamlNode("RequiresCondition", "enabled"));

            actorNode.Children.Add(customTraitNode);

            return actorNode;
        }
    }
}