using System;
using System.IO;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Models;
using DemoPulse.Services;
using DemoPulse.Services.Providers;

namespace DemoPulse.Interop.Handlers
{
    public class GetSettingsHandler : ICommandHandler
    {
        private readonly IUiMessenger _messenger;
        private readonly AppSettings _settings;

        public GetSettingsHandler(IUiMessenger messenger, AppSettings settings)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string CommandName => "GET_SETTINGS";

        public void Execute(IpcRequest request)
        {
            _messenger.SendResponse(new IpcResponse
            {
                Id = request.Id,
                Type = "SETTINGS_DATA",
                Success = true,
                Payload = _settings
            });
        }
    }

    public class SaveSettingsHandler : ICommandHandler
    {
        private readonly IUiMessenger _messenger;
        private readonly AppSettings _settings;

        public SaveSettingsHandler(IUiMessenger messenger, AppSettings settings)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string CommandName => "SAVE_SETTINGS";

        public void Execute(IpcRequest request)
        {
            try
            {
                AppSettings? updated = null;

                if (request.Payload.ValueKind == JsonValueKind.Object)
                {
                    updated = JsonSerializer.Deserialize<AppSettings>(request.Payload.GetRawText());
                }
                else if (request.Payload.ValueKind == JsonValueKind.String)
                {
                    string rawJson = request.Payload.GetString() ?? "";
                    if (!string.IsNullOrWhiteSpace(rawJson))
                        updated = JsonSerializer.Deserialize<AppSettings>(rawJson);
                }

                if (updated != null)
                {
                    _settings.ConfigFileName = updated.ConfigFileName;
                    _settings.Cs2CfgFolder = updated.Cs2CfgFolder;
                    _settings.KeyBindT = updated.KeyBindT;
                    _settings.KeyBindCT = updated.KeyBindCT;
                    _settings.KeyBindAll = updated.KeyBindAll;
                    _settings.KeyBindMute = updated.KeyBindMute;
                    _settings.KeyBindSpeedUp = updated.KeyBindSpeedUp;
                    _settings.KeyBindSlowMo = updated.KeyBindSlowMo;
                    _settings.KeyBindPause = updated.KeyBindPause;
                    _settings.KeyBindResetSpeed = updated.KeyBindResetSpeed;
                    _settings.AutoSaveToCs2 = updated.AutoSaveToCs2;

                    _settings.Save();
                    _messenger.SendResponse(new IpcResponse
                    {
                        Id = request.Id,
                        Type = "SETTINGS_SAVED",
                        Success = true,
                        Payload = _settings
                    });
                }
            }
            catch (Exception ex)
            {
                _messenger.SendResponse(new IpcResponse
                {
                    Id = request.Id,
                    Type = "SETTINGS_ERROR",
                    Success = false,
                    Error = ex.Message
                });
            }
        }
    }

    public class BrowseCs2FolderHandler : ICommandHandler
    {
        private readonly IDialogService _dialogService;
        private readonly IUiMessenger _messenger;
        private readonly AppSettings _settings;
        private readonly IFileSystemService _fileSystem;

        public BrowseCs2FolderHandler(IDialogService dialogService, IUiMessenger messenger, AppSettings settings, IFileSystemService? fileSystem = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileSystem = fileSystem ?? new PhysicalFileSystemService();
        }

        public string CommandName => "BROWSE_CS2_FOLDER";

        public void Execute(IpcRequest request)
        {
            string? selectedFile = _dialogService.ShowOpenFileDialog(
                "Select CS2 cfg folder (Select any file in CS2's game/csgo/cfg folder)",
                "CS2 Config Files (*.cfg)|*.cfg|All Files (*.*)|*.*",
                defaultFileName: "Select Folder",
                checkFileExists: false
            );

            if (!string.IsNullOrEmpty(selectedFile))
            {
                string? folder = Path.GetDirectoryName(selectedFile);
                if (!string.IsNullOrEmpty(folder) && _fileSystem.DirectoryExists(folder))
                {
                    _settings.Cs2CfgFolder = folder;
                    _settings.Save();
                    _messenger.SendResponse(new IpcResponse
                    {
                        Id = request.Id,
                        Type = "SETTINGS_DATA",
                        Success = true,
                        Payload = _settings
                    });
                }
            }
        }
    }
}
