using WILK.Models;

namespace WILK.Services
{
    /// <summary>
    /// Service for processing external files (CSV, Excel, JSON)
    /// </summary>
    public interface IFileProcessingService
    {
        /// <returns>List of component IDs as strings</returns>
        List<string> LoadCSV(string path);
        /// <returns>Tuple containing list name and component data</returns>
        (string LoadedListName, List<(string Kol1, string Kol2, string Kol3)>) LoadList(string path);
        /// <returns>List of tuples containing component ID and alternative ID</returns>
        List<(string id, string altId)> LoadAltsList(string path);
        /// <returns>List of container IDs</returns>
        List<string> LoadJson(string path);
    }
}