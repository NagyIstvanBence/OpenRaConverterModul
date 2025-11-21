using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OpenRA.Converter.Core.Interfaces;
using OpenRA.Converter.Core.Models;

namespace OpenRA.Converter.Infrastructure.Services
{
    /// <summary>
    /// Thread-safe in-memory storage for Trait and Weapon definitions.
    /// This should be registered as a Singleton.
    /// </summary>
    public class ReferenceRegistry : IReferenceRegistry
    {
        private readonly ConcurrentDictionary<string, TraitSchema> _traits = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, WeaponSchema> _weapons = new(StringComparer.OrdinalIgnoreCase);

        public void RegisterTraits(IEnumerable<TraitSchema> traits)
        {
            foreach (var trait in traits)
            {
                _traits[trait.Name] = trait;
            }
        }

        public void RegisterWeapons(IEnumerable<WeaponSchema> weapons)
        {
            foreach (var weapon in weapons)
            {
                _weapons[weapon.Type] = weapon;
            }
        }

        public TraitSchema? GetTrait(string name)
        {
            _traits.TryGetValue(name, out var trait);
            return trait;
        }

        public WeaponSchema? GetWeaponType(string type)
        {
            _weapons.TryGetValue(type, out var weapon);
            return weapon;
        }

        public IEnumerable<string> GetAllTraitNames()
        {
            return _traits.Keys;
        }
    }
}