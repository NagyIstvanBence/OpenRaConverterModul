using Microsoft.AspNetCore.Mvc;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Infrastructure.Services;
using System;
using System.Text.Json;

namespace OpenRA.Converter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SynthesisController : ControllerBase
    {
        private readonly IDecisionTreeService _treeService;
        private readonly ITraitSynthesisService _csharpSynthesisService;
        private readonly IYamlSynthesisService _yamlSynthesisService;
        private readonly ICodeWriter _csharpWriter;
        private readonly IYamlCodeWriter _yamlWriter;

        public SynthesisController(
            IDecisionTreeService treeService,
            ITraitSynthesisService csharpSynthesisService,
            IYamlSynthesisService yamlSynthesisService,
            ICodeWriter csharpWriter,
            IYamlCodeWriter yamlWriter)
        {
            _treeService = treeService;
            _csharpSynthesisService = csharpSynthesisService;
            _yamlSynthesisService = yamlSynthesisService;
            _csharpWriter = csharpWriter;
            _yamlWriter = yamlWriter;
        }

        [HttpPost("generate-csharp")]
        public IActionResult GenerateCSharp([FromBody] JsonElement payload, [FromQuery] string traitName = "NewTrait")
        {
            try
            {
                var rootNode = _treeService.ParseTree(payload);
                var validationErrors = _treeService.ValidateTree(rootNode);
                if (validationErrors.Count > 0) return BadRequest(new { Errors = validationErrors });

                var classStructure = _csharpSynthesisService.SynthesizeTrait(rootNode, traitName);
                var code = _csharpWriter.WriteClass(classStructure);

                return Ok(new { FileName = $"{traitName}.cs", Code = code });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPost("generate-yaml")]
        public IActionResult GenerateYaml([FromBody] JsonElement payload, [FromQuery] string actorName = "MyActor", [FromQuery] string traitName = "NewTrait")
        {
            try
            {
                var rootNode = _treeService.ParseTree(payload);
                // We validate the tree even for YAML to ensure the logic structure is sound before binding
                var validationErrors = _treeService.ValidateTree(rootNode);
                if (validationErrors.Count > 0) return BadRequest(new { Errors = validationErrors });

                // Synthesize YAML AST
                var yamlStructure = _yamlSynthesisService.SynthesizeActor(rootNode, actorName, traitName);

                // Write to String
                var yamlCode = _yamlWriter.WriteYaml(yamlStructure);

                return Ok(new { FileName = $"{actorName.ToLower()}.yaml", Code = yamlCode });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}