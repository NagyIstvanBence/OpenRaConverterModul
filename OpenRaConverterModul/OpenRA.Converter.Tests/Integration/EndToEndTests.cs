using System.Collections.Generic;
using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Infrastructure.Services;
using OpenRA.Converter.Infrastructure.ReferenceModels; // For real registry if needed
using Xunit;

namespace OpenRA.Converter.Tests.Integration
{
    public class EndToEndTests
    {
        [Fact]
        public void FullPipeline_ShouldGenerateValidCode()
        {
            // 1. Setup Real Infrastructure (No Mocks)
            var registry = new ReferenceRegistry();
            var treeService = new DecisionTreeService(registry);
            var traitService = new TraitSynthesisService(treeService, registry);
            var codeWriter = new CSharpCodeWriter();

            // 2. Define Input (A simple Flee behavior)
            var rootNode = new DecisionNode
            {
                Id = "root",
                Condition = "Health < 50%",
                Children = new List<DecisionNode>
                {
                    new DecisionNode { Id = "flee", Action = "Move(Base)" }
                }
            };

            // 3. Execute Pipeline
            var classStructure = traitService.SynthesizeTrait(rootNode, "FleeBehavior");
            var code = codeWriter.WriteClass(classStructure);

            // 4. Assertions (Check for key generated patterns)
            Assert.Contains("class FleeBehavior", code);
            Assert.Contains("class FleeBehaviorInfo", code);
            // Verify logic generation
            Assert.Contains("if (self.Trait<Health>().HP <", code);
            // Verify action generation
            Assert.Contains("QueueActivity(new Move", code);
            // Verify standard OpenRA boilerplate
            Assert.Contains("if (IsTraitDisabled) return;", code);
        }
    }
}