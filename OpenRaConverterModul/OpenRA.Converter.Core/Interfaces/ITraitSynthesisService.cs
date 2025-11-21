using OpenRA.Converter.Core.Models.CodeStructure;
using OpenRA.Converter.Core.Models.DecisionTree;

namespace OpenRA.Converter.Core.Interfaces
{
    /// <summary>
    /// Responsible for converting a Decision Tree into C# Code Models.
    /// </summary>
    public interface ITraitSynthesisService
    {
        /// <summary>
        /// Analyzes a decision tree and generates the corresponding OpenRA Trait C# class structure.
        /// This includes generating the Info class (configuration) and the Logic class (behavior).
        /// </summary>
        /// <param name="rootNode">The root of the decision tree.</param>
        /// <param name="traitName">The desired name for the trait (e.g., "FireFighter").</param>
        /// <returns>The main logic class, which contains a reference to its paired Info class.</returns>
        CsClass SynthesizeTrait(DecisionNode rootNode, string traitName);
    }
}