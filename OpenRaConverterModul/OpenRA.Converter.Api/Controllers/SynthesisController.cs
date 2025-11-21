using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;

namespace OpenRA.Converter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SynthesisController : ControllerBase
    {
        private readonly IDecisionTreeService _treeService;
        private readonly ITraitSynthesisService _synthesisService;
        private readonly ICodeWriter _codeWriter;

        public SynthesisController(
            IDecisionTreeService treeService,
            ITraitSynthesisService synthesisService,
            ICodeWriter codeWriter)
        {
            _treeService = treeService;
            _synthesisService = synthesisService;
            _codeWriter = codeWriter;
        }

        /// <summary>
        /// Converts a Decision Tree JSON into C# source code.
        /// </summary>
        /// <param name="payload">The decision tree JSON.</param>
        /// <param name="traitName">The name of the class to generate (e.g. "FireFighter").</param>
        /// <returns>The generated C# file content.</returns>
        [HttpPost("generate-csharp")]
        public IActionResult GenerateCSharp([FromBody] JsonElement payload, [FromQuery] string traitName = "NewTrait")
        {
            try
            {
                // 1. Parse the Tree
                var rootNode = _treeService.ParseTree(payload);
                var validationErrors = _treeService.ValidateTree(rootNode);

                if (validationErrors.Count > 0)
                {
                    return BadRequest(new { Errors = validationErrors });
                }

                // 2. Synthesize AST (Abstract Syntax Tree)
                var classStructure = _synthesisService.SynthesizeTrait(rootNode, traitName);

                // 3. Write to String
                var code = _codeWriter.WriteClass(classStructure);

                return Ok(new { FileName = $"{traitName}.cs", Code = code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}