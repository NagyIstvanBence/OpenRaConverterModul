using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenRA.Converter.Infrastructure.ReferenceModels
{
    /// <summary>
    /// Represents the root object of the JSON request for reference ingestion.
    /// </summary>
    public class RawReferenceRequest
    {
        // The JSON might come as { "Data": [...] } or just [...] depending on the source.
        // We handle the mapping logic in the service, but this DTO is for the { "Data": ... } wrapper case.
        [JsonPropertyName("Data")]
        public List<object>? Data { get; set; }
    }

    /// <summary>
    /// Represents a single row in the raw traits.json file.
    /// </summary>
    public class RawTraitRow
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("traitname")]
        public string TraitName { get; set; } = string.Empty;

        [JsonPropertyName("property")]
        public string Property { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("defaultvalue")]
        public string? DefaultValue { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("inherittraits")]
        public string? InheritTraits { get; set; }

        [JsonPropertyName("requiretraits")]
        public string? RequireTraits { get; set; }
    }

    /// <summary>
    /// Represents a single row in the raw weapons.json file.
    /// </summary>
    public class RawWeaponRow
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("weapontype")]
        public string WeaponType { get; set; } = string.Empty;

        [JsonPropertyName("weapontypedescription")]
        public string? WeaponTypeDescription { get; set; }

        [JsonPropertyName("property")]
        public string Property { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("defaultvalue")]
        public string? DefaultValue { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}