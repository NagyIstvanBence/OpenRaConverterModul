using OpenRA.Converter.Core.Models.CodeStructure;
using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Core.Interfaces
{
    public interface IYamlSynthesisService
    {
        /// <summary>
        /// Generates a complete Actor definition YAML.
        /// </summary>
        /// <param name="rootNode">The decision tree root.</param>
        /// <param name="generatedTrait">The synthesized C# class (contains detected fields/dependencies).</param>
        /// <param name="actorName">The name of the actor (e.g., "BAZOOKA_GUY").</param>
        YamlNode SynthesizeActor(DecisionNode rootNode, CsClass generatedTrait, string actorName);
    }
}