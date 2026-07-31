using System.Threading.Tasks;

namespace DemoPulse.Services
{
    /// <summary>
    /// Service abstraction for initiating asynchronous CS2 match demo parsing.
    /// </summary>
    public interface IDemoService
    {
        /// <summary>
        /// Asynchronously parses a CS2 demo file at the specified path.
        /// </summary>
        Task LoadDemoPathAsync(string filePath);

        Task LoadDemoPathAsync(string filePath, string? requestId);

        /// <summary>
        /// Gets the most recently parsed demo match data (or null if none).
        /// </summary>
        Models.Dto.MatchDataDto? CurrentMatchData { get; }
    }
}
