using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Core.Models.YamlStructure;

namespace OpenRA.Converter.Core.Interfaces
{
    public interface IYamlSynthesisService
    {
        /// <summary>
        /// Generates a complete Actor definition YAML containing the standard boilerplate
        /// and attaching the generated custom trait.
        /// </summary>
        /// <param name="rootNode">The decision tree root (used to analyze requirements).</param>
        /// <param name="actorName">The name of the actor (e.g., "BAZOOKA_GUY").</param>
        /// <param name="traitName">The name of the generated C# trait to attach.</param>
        YamlNode SynthesizeActor(DecisionNode rootNode, string actorName, string traitName);
    }
}