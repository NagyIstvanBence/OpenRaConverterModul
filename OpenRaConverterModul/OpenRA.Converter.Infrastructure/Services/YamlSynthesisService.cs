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

            // 2. Auto-detected Dependencies
            if (generatedTrait.RequiredYamlInherits.Count > 0)
            {
                var deps = string.Join(", ", generatedTrait.RequiredYamlInherits.Distinct());
                actorNode.Children.Add(new YamlNode("Comment", $"Detected Dependencies: {deps}") { Comment = "Logic requirements" });
            }

            // 3. The Custom Trait
            // This Key will match the C# class name (e.g. BazookaLogic)
            var customTraitNode = new YamlNode(generatedTrait.Name);

            // 4. Dynamic Parameters
            if (generatedTrait.PairedInfoClass != null)
            {
                foreach (var field in generatedTrait.PairedInfoClass.Fields.Where(f => f.IsExposedToYaml))
                {
                    // Skip RequiresCondition to avoid disabling the trait accidentally
                    if (field.Name == "RequiresCondition") continue;

                    string val = field.InitialValue?.Replace("\"", "") ?? "";

                    // If int is missing, default to 0. If string is missing, keep empty.
                    if (field.Type == "int" && string.IsNullOrEmpty(val)) val = "0";
                    if (field.Type == "bool" && string.IsNullOrEmpty(val)) val = "false";

                    customTraitNode.Children.Add(new YamlNode(field.Name, val));
                }
            }

            actorNode.Children.Add(customTraitNode);

            return actorNode;
        }
    }
}