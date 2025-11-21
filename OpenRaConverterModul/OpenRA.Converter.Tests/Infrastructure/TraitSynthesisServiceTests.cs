using System.Linq;
using Moq;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models;
using OpenRA.Converter.Core.Models.DecisionTree;
using OpenRA.Converter.Infrastructure.Services;
using Xunit;

namespace OpenRA.Converter.Tests.Infrastructure
{
    public class TraitSynthesisServiceTests
    {
        private readonly TraitSynthesisService _synthesisService;
        private readonly Mock<IDecisionTreeService> _mockTreeService;
        private readonly Mock<IReferenceRegistry> _mockRegistry;

        public TraitSynthesisServiceTests()
        {
            _mockTreeService = new Mock<IDecisionTreeService>();
            _mockRegistry = new Mock<IReferenceRegistry>();

            // Default mock behavior for parsing simple strings if needed
            _mockTreeService.Setup(s => s.ParseConditionString(It.IsAny<string>()))
                .Returns((string s) => new ParsedCondition { Variable = s });

            _synthesisService = new TraitSynthesisService(_mockTreeService.Object, _mockRegistry.Object);
        }

        [Fact]
        public void SynthesizeTrait_ShouldAddMobileDependency_WhenActionIsMove()
        {
            // Arrange
            var rootNode = new DecisionNode
            {
                Id = "root",
                Action = "Move(Home)"
            };

            // Act
            var result = _synthesisService.SynthesizeTrait(rootNode, "TestTrait");

            // Assert
            Assert.Contains("Mobile", result.RequiredYamlInherits);
        }

        [Fact]
        public void SynthesizeTrait_ShouldGenerateParameterField_WhenVariableIsUnknown()
        {
            // Arrange
            var rootNode = new DecisionNode { Id = "root", Condition = "IsBrave" };

            // Setup mock to return a condition that isn't in the registry/defaults
            _mockTreeService.Setup(s => s.ParseConditionString("IsBrave"))
                .Returns(new ParsedCondition { Variable = "IsBrave" });

            // Act
            var result = _synthesisService.SynthesizeTrait(rootNode, "TestTrait");

            // Assert
            var infoClass = result.PairedInfoClass;
            Assert.NotNull(infoClass);
            // Expect a field named 'IsBrave' to be created in the Info class
            Assert.Contains(infoClass.Fields, f => f.Name == "IsBrave" && f.IsExposedToYaml);
        }
    }
}