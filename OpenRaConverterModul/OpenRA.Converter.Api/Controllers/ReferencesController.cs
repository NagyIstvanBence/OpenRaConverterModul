using Microsoft.AspNetCore.Mvc;
using OpenRA.Converter.Core.Interfaces;
using System.Text.Json;

namespace OpenRA.Converter.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferencesController : ControllerBase
    {
        private readonly IReferenceIngestionService _ingestionService;
        private readonly IReferenceRegistry _registry;

        public ReferencesController(IReferenceIngestionService ingestionService, IReferenceRegistry registry)
        {
            _ingestionService = ingestionService;
            _registry = registry;
        }

        /// <summary>
        /// Uploads and parses the 'traits.json' reference table.
        /// Input structure: { "Data": [ ... ] }
        /// </summary>
        [HttpPost("traits")]
        public IActionResult IngestTraits([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("Data", out var dataElement))
            {
                // Handle case where "data" might be lowercase (JsonElement is case sensitive usually depending on config)
                if (!payload.TryGetProperty("data", out dataElement))
                {
                    return BadRequest("Missing 'Data' field in JSON body.");
                }
            }

            try
            {
                var count = _ingestionService.IngestTraits(dataElement);
                return Ok(new { Message = $"Successfully ingested {count} traits." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Uploads and parses the 'weapons.json' reference table.
        /// Input structure: { "Data": [ ... ] }
        /// </summary>
        [HttpPost("weapons")]
        public IActionResult IngestWeapons([FromBody] JsonElement payload)
        {
            if (!payload.TryGetProperty("Data", out var dataElement))
            {
                if (!payload.TryGetProperty("data", out dataElement))
                {
                    return BadRequest("Missing 'Data' field in JSON body.");
                }
            }

            try
            {
                var count = _ingestionService.IngestWeapons(dataElement);
                return Ok(new { Message = $"Successfully ingested {count} weapon types." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Debug endpoint to verify a specific trait exists in the system.
        /// </summary>
        [HttpGet("traits/{name}")]
        public IActionResult GetTrait(string name)
        {
            var trait = _registry.GetTrait(name);
            if (trait == null) return NotFound($"Trait '{name}' not found.");
            return Ok(trait);
        }

        /// <summary>
        /// Debug endpoint to verify a specific weapon type exists in the system.
        /// </summary>
        [HttpGet("weapons/{type}")]
        public IActionResult GetWeapon(string type)
        {
            var weapon = _registry.GetWeaponType(type);
            if (weapon == null) return NotFound($"Weapon Type '{type}' not found.");
            return Ok(weapon);
        }
    }
}