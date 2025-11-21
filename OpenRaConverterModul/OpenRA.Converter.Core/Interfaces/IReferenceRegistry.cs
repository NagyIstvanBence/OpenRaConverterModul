using System.Collections.Generic;
using OpenRA.Converter.Core.Models;

namespace OpenRA.Converter.Core.Interfaces
{
    /// <summary>
    /// Provides fast lookup access to normalized Trait and Weapon definitions.
    /// acts as the "Truth Source" for the system.
    /// </summary>
    public interface IReferenceRegistry
    {
        /// <summary>
        /// Stores a list of traits in the in-memory registry.
        /// </summary>
        void RegisterTraits(IEnumerable<TraitSchema> traits);

        /// <summary>
        /// Stores a list of weapons in the in-memory registry.
        /// </summary>
        void RegisterWeapons(IEnumerable<WeaponSchema> weapons);

        /// <summary>
        /// Retrieves a trait definition by name (case-insensitive).
        /// </summary>
        TraitSchema? GetTrait(string name);

        /// <summary>
        /// Retrieves a weapon type definition by name (case-insensitive).
        /// </summary>
        WeaponSchema? GetWeaponType(string type);

        /// <summary>
        /// Returns all registered trait names.
        /// </summary>
        IEnumerable<string> GetAllTraitNames();
    }
}