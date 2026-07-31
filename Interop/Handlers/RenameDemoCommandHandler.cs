using System;
using System.IO;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Services;

namespace DemoPulse.Interop.Handlers
{
    public class RenameDemoHandler : ICommandHandler
    {
        private readonly IUiMessenger _uiMessenger;
        private readonly IDialogService _dialogService;

        public RenameDemoHandler(IUiMessenger uiMessenger, IDialogService dialogService)
        {
            _uiMessenger = uiMessenger ?? throw new ArgumentNullException(nameof(uiMessenger));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        public string CommandName => "RENAME_DEMO";

        public void Execute(IpcRequest request)
        {
            string currentPath = "";
            string newName = "";

            if (request.Payload.ValueKind == JsonValueKind.Object)
            {
                if (request.Payload.TryGetProperty("currentPath", out var pathProp))
                    currentPath = pathProp.GetString() ?? "";
                if (request.Payload.TryGetProperty("newName", out var nameProp))
                    newName = nameProp.GetString() ?? "";
            }

            if (string.IsNullOrWhiteSpace(currentPath) || !File.Exists(currentPath))
            {
                _uiMessenger.SendResponse(IpcResponse.Fail(request.Id, request.Command, "Source demo file does not exist."));
                return;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                _uiMessenger.SendResponse(IpcResponse.Fail(request.Id, request.Command, "New file name cannot be empty."));
                return;
            }

            try
            {
                string sanitized = AppSettings.SanitizeConfigFileName(newName);
                if (!sanitized.EndsWith(".dem", StringComparison.OrdinalIgnoreCase))
                {
                    sanitized += ".dem";
                }

                string dir = Path.GetDirectoryName(currentPath) ?? "";
                string newPath = Path.Combine(dir, sanitized);

                if (string.Equals(currentPath, newPath, StringComparison.OrdinalIgnoreCase))
                {
                    _uiMessenger.SendResponse(IpcResponse.Ok(request.Id, request.Command, new
                    {
                        success = true,
                        oldPath = currentPath,
                        newPath = currentPath,
                        newFileName = Path.GetFileName(currentPath)
                    }));
                    return;
                }

                if (File.Exists(newPath))
                {
                    _uiMessenger.SendResponse(IpcResponse.Fail(request.Id, request.Command, $"A file named '{sanitized}' already exists in the folder."));
                    return;
                }

                File.Move(currentPath, newPath);

                var payload = new
                {
                    success = true,
                    oldPath = currentPath,
                    newPath = newPath,
                    newFileName = Path.GetFileName(newPath)
                };

                _uiMessenger.SendResponse(IpcResponse.Ok(request.Id, request.Command, payload));
                _uiMessenger.PostMessage(JsonSerializer.Serialize(new
                {
                    type = "DEMO_RENAMED",
                    payload = payload
                }));
            }
            catch (Exception ex)
            {
                _uiMessenger.SendResponse(IpcResponse.Fail(request.Id, request.Command, $"Failed to rename demo file: {ex.Message}"));
            }
        }
    }
}
