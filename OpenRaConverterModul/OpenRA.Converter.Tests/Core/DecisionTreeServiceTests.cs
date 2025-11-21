using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;
using Moq;
using Xunit;

namespace OpenRA.Converter.Tests.Core
{
    public class DecisionTreeServiceTests
    {
        private readonly DecisionTreeService _service;
        private readonly Mock<IReferenceRegistry> _mockRegistry;

        public DecisionTreeServiceTests()
        {
            _mockRegistry = new Mock<IReferenceRegistry>();
            _service = new DecisionTreeService(_mockRegistry.Object);
        }

        [Theory]
        [InlineData("Health < 30%", "Health", "<", "30%")]
        [InlineData("EnemyVisible", "EnemyVisible", null, null)]
        [InlineData("Not EnemyVisible", "EnemyVisible", null, null, true)]
        [InlineData("!HasRocket", "HasRocket", null, null, true)]
        [InlineData("Ammo > 5", "Ammo", ">", "5")]
        public void ParseConditionString_ShouldParseCorrectly(string input, string expectedVar, string? expectedOp, string? expectedVal, bool expectedNegation = false)
        {
            // Act
            var result = _service.ParseConditionString(input);

            // Assert
            Assert.Equal(expectedVar, result.Variable);
            Assert.Equal(expectedOp, result.Operator);
            Assert.Equal(expectedVal, result.Value);
            Assert.Equal(expectedNegation, result.IsNegated);
        }
    }
}