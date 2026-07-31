using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Services;

namespace DemoPulse.Interop.Handlers
{
    public class OpenFolderHandler : ICommandHandler
    {
        private readonly IDialogService _dialogService;

        public OpenFolderHandler(IDialogService dialogService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public string CommandName => "OPEN_DEMO_FOLDER";

        public void Execute(IpcRequest request)
        {
            string filePath = "";

            if (request.Payload.ValueKind == JsonValueKind.Object && request.Payload.TryGetProperty("filePath", out var pathProp))
            {
                filePath = pathProp.GetString() ?? "";
            }
            else if (request.Payload.ValueKind == JsonValueKind.String)
            {
                filePath = request.Payload.GetString() ?? "";
            }

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _dialogService.ShowMessageBox("Demo file does not exist on disk.", "DemoPulse", isWarning: true);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{filePath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessageBox($"Could not open file location: {ex.Message}", "DemoPulse Error", isWarning: true);
            }
        }
    }
}
