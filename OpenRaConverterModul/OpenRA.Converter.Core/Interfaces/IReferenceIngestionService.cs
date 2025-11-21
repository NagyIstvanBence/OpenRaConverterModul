using System.Text.Json;

namespace OpenRA.Converter.Core.Interfaces
{
    /// <summary>
    /// Handles the logic of parsing raw JSON data into normalized Domain Schemas.
    /// </summary>
    public interface IReferenceIngestionService
    {
        /// <summary>
        /// Parses raw JSON containing trait rows and populates the registry.
        /// </summary>
        /// <param name="jsonElement">The "Data" array from the API request.</param>
        /// <returns>The count of traits successfully processed.</returns>
        int IngestTraits(JsonElement jsonElement);

        /// <summary>
        /// Parses raw JSON containing weapon rows and populates the registry.
        /// </summary>
        /// <param name="jsonElement">The "Data" array from the API request.</param>
        /// <returns>The count of weapon types successfully processed.</returns>
        int IngestWeapons(JsonElement jsonElement);
    }
}