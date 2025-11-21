using System.Text.Json.Serialization;

namespace OpenRA.Converter.Infrastructure.ReferenceModels
{
    // DTOs for deserialization matching the provided JSON files exactly.

    public class RawReferenceRequest
    {
        [JsonPropertyName("Data")]
        public List<object>? Data { get; set; }
    }

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