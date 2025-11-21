using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models;
using OpenRA.Converter.Infrastructure.ReferenceModels;

namespace OpenRA.Converter.Infrastructure.Services
{
    public class ReferenceIngestionService : IReferenceIngestionService
    {
        private readonly IReferenceRegistry _registry;

        public ReferenceIngestionService(IReferenceRegistry registry)
        {
            _registry = registry;
        }

        public int IngestTraits(JsonElement jsonElement)
        {
            // Deserialize raw rows
            var rawRows = JsonSerializer.Deserialize<List<RawTraitRow>>(jsonElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rawRows == null || !rawRows.Any()) return 0;

            // Group by TraitName to normalize the flat list into objects
            var traitGroups = rawRows
                .Where(r => !string.IsNullOrWhiteSpace(r.TraitName))
                .GroupBy(r => r.TraitName);

            var normalizedTraits = new List<TraitSchema>();

            foreach (var group in traitGroups)
            {
                var representative = group.First();

                var schema = new TraitSchema
                {
                    Name = representative.TraitName,
                    InheritedTraits = ParseCommaSeparatedList(representative.InheritTraits),
                    RequiredTraits = ParseCommaSeparatedList(representative.RequireTraits)
                };

                // Map properties
                foreach (var row in group)
                {
                    if (string.IsNullOrWhiteSpace(row.Property)) continue;

                    schema.Properties[row.Property] = new PropertyDefinition
                    {
                        Name = row.Property,
                        Type = row.Type,
                        DefaultValue = row.DefaultValue,
                        Description = row.Description
                    };
                }

                normalizedTraits.Add(schema);
            }

            _registry.RegisterTraits(normalizedTraits);
            return normalizedTraits.Count;
        }

        public int IngestWeapons(JsonElement jsonElement)
        {
            var rawRows = JsonSerializer.Deserialize<List<RawWeaponRow>>(jsonElement.GetRawText(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (rawRows == null || !rawRows.Any()) return 0;

            // Group by WeaponType
            var weaponGroups = rawRows
                .Where(r => !string.IsNullOrWhiteSpace(r.WeaponType))
                .GroupBy(r => r.WeaponType);

            var normalizedWeapons = new List<WeaponSchema>();

            foreach (var group in weaponGroups)
            {
                var representative = group.First();

                var schema = new WeaponSchema
                {
                    Type = representative.WeaponType,
                    Description = representative.WeaponTypeDescription ?? string.Empty
                };

                foreach (var row in group)
                {
                    if (string.IsNullOrWhiteSpace(row.Property)) continue;

                    schema.Properties[row.Property] = new PropertyDefinition
                    {
                        Name = row.Property,
                        Type = row.Type,
                        DefaultValue = row.DefaultValue,
                        Description = row.Description
                    };
                }

                normalizedWeapons.Add(schema);
            }

            _registry.RegisterWeapons(normalizedWeapons);
            return normalizedWeapons.Count;
        }

        private List<string> ParseCommaSeparatedList(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();

            return input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();
        }
    }
}