using Microsoft.AspNetCore.Mvc;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models.DecisionTree;
using System.Text.Json;

namespace OpenRA.Converter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecisionTreesController : ControllerBase
    {
        private readonly IDecisionTreeService _treeService;

        public DecisionTreesController(IDecisionTreeService treeService)
        {
            _treeService = treeService;
        }

        /// <summary>
        /// Parses and validates a Decision Tree JSON.
        /// Returns the parsed structure and any validation errors.
        /// </summary>
        [HttpPost("parse")]
        public IActionResult ParseTree([FromBody] JsonElement payload)
        {
            try
            {
                var tree = _treeService.ParseTree(payload);
                var validationErrors = _treeService.ValidateTree(tree);

                if (validationErrors.Count > 0)
                {
                    return BadRequest(new
                    {
                        Message = "Tree parsed but validation failed.",
                        Errors = validationErrors,
                        ParsedTree = tree
                    });
                }

                return Ok(new
                {
                    Message = "Decision Tree is valid.",
                    ParsedTree = tree
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test endpoint to see how the engine parses a specific condition string.
        /// </summary>
        [HttpGet("debug-condition")]
        public IActionResult DebugCondition([FromQuery] string condition)
        {
            var result = _treeService.ParseConditionString(condition);
            return Ok(result);
        }
    }
}