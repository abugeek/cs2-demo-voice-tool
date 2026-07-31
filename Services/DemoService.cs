using System;
using System.Threading.Tasks;
using DemoPulse.Interop.Contracts;
using DemoPulse.Models.Dto;
using DemoPulse.Services.Providers;

namespace DemoPulse.Services
{
    public class DemoService : IDemoService
    {
        private readonly IUiMessenger _messenger;
        private readonly IFileStreamProvider _streamProvider;

        public MatchDataDto? CurrentMatchData { get; private set; }

        public DemoService(IUiMessenger messenger, IFileStreamProvider? streamProvider = null)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _streamProvider = streamProvider ?? new PhysicalFileSystemService();
        }

        public Task LoadDemoPathAsync(string filePath)
        {
            return LoadDemoPathAsync(filePath, null);
        }

        public async Task LoadDemoPathAsync(string filePath, string? requestId)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return;

            if (System.IO.Directory.Exists(filePath))
            {
                string folderName = System.IO.Path.GetFileName(filePath);
                _messenger.SendResponse(new IpcResponse
                {
                    Id = requestId,
                    Type = "DEMO_PARSE_ERROR",
                    Success = false,
                    Error = $"Target path '{folderName}' is a folder, not a .dem demo file."
                });
                return;
            }

            if (!System.IO.File.Exists(filePath))
            {
                _messenger.SendResponse(new IpcResponse
                {
                    Id = requestId,
                    Type = "DEMO_PARSE_ERROR",
                    Success = false,
                    Error = $"Demo file does not exist: '{filePath}'"
                });
                return;
            }

            if (!filePath.EndsWith(".dem", StringComparison.OrdinalIgnoreCase))
            {
                string folderName = System.IO.Path.GetFileName(filePath);
                _messenger.SendResponse(new IpcResponse
                {
                    Id = requestId,
                    Type = "DEMO_PARSE_ERROR",
                    Success = false,
                    Error = $"Target file '{folderName}' is not a valid .dem file."
                });
                return;
            }

            // Notify UI to show loading spinner
            _messenger.SendResponse(new IpcResponse
            {
                Id = requestId,
                Type = "DEMO_PARSING_START",
                Success = true
            });

            try
            {
                // Parse on a background thread using injectable stream provider
                MatchDataDto matchData = await Task.Run(async () => await DemoParserService.ParseDemoMatchDataAsync(filePath, _streamProvider));
                CurrentMatchData = matchData;
                
                _messenger.SendResponse(new IpcResponse
                {
                    Id = requestId,
                    Type = "DEMO_DATA",
                    Success = true,
                    Payload = matchData
                });
            }
            catch (Exception ex)
            {
                string safeMsg = ex.Message.Replace("\"", "'").Replace("\n", " ").Replace("\r", "");
                _messenger.SendResponse(new IpcResponse
                {
                    Id = requestId,
                    Type = "DEMO_PARSE_ERROR",
                    Success = false,
                    Error = safeMsg
                });
            }
        }
    }
}
