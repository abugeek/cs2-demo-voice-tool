using System;
using System.IO;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Models;
using DemoPulse.Services;
using DemoPulse.Services.Providers;

namespace DemoPulse.Interop.Handlers
{
    public class GenerateVoiceCfgHandler : ICommandHandler
    {
        private readonly IUiMessenger _messenger;
        private readonly AppSettings _settings;
        private readonly IFileSystemService _fileSystem;

        public GenerateVoiceCfgHandler(IUiMessenger messenger, AppSettings settings, IFileSystemService? fileSystem = null)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileSystem = fileSystem ?? new PhysicalFileSystemService();
        }

        public string CommandName => "GENERATE_VOICE_CFG";

        public void Execute(IpcRequest request)
        {
            var (mode, customMask, tMask, ctMask) = ExtractVoiceParams(request.Payload);

            string cfg = VoiceBitmaskService.GenerateCS2Config(mode, customMask, tMask, ctMask, _settings);
            _messenger.SendResponse(new IpcResponse
            {
                Id = request.Id,
                Type = "VOICE_CFG_RESULT",
                Success = true,
                Payload = new { configText = cfg }
            });

            if (!AutoSaveConfigToCs2Folder(cfg, _settings, _fileSystem, out string? autoSaveError))
            {
                _messenger.SendResponse(new IpcResponse
                {
                    Id = request.Id,
                    Type = "AUTOSAVE_ERROR",
                    Success = false,
                    Error = autoSaveError
                });
            }
        }

        public static bool AutoSaveConfigToCs2Folder(string cfgContent, AppSettings settings, out string? errorMessage)
        {
            return AutoSaveConfigToCs2Folder(cfgContent, settings, new PhysicalFileSystemService(), out errorMessage);
        }

        internal static bool AutoSaveConfigToCs2Folder(string cfgContent, AppSettings settings, IFileSystemService fileSystem, out string? errorMessage)
        {
            errorMessage = null;

            if (settings.AutoSaveToCs2 && !string.IsNullOrWhiteSpace(settings.Cs2CfgFolder))
            {
                if (!fileSystem.DirectoryExists(settings.Cs2CfgFolder))
                {
                    errorMessage = $"CS2 cfg folder does not exist: '{settings.Cs2CfgFolder}'";
                    return false;
                }

                try
                {
                    string safeBaseName = AppSettings.SanitizeConfigFileName(settings.ConfigFileName);
                    string cfgName = safeBaseName + ".cfg";

                    string destPath = Path.Combine(settings.Cs2CfgFolder, cfgName);
                    string fullTarget = Path.GetFullPath(destPath);
                    string fullFolder = Path.GetFullPath(settings.Cs2CfgFolder);

                    if (!fullTarget.StartsWith(fullFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        errorMessage = $"Security Error: Config file path '{destPath}' escapes target folder '{settings.Cs2CfgFolder}'";
                        return false;
                    }

                    fileSystem.WriteAllText(destPath, cfgContent);
                    return true;
                }
                catch (Exception ex)
                {
                    errorMessage = $"Failed to auto-save CS2 config to '{settings.Cs2CfgFolder}': {ex.Message}";
                    return false;
                }
            }

            return true;
        }

        internal static (string mode, ulong? customMask, ulong? tMask, ulong? ctMask) ExtractVoiceParams(JsonElement payload)
        {
            string mode = "ALL";
            ulong? customMask = null;
            ulong? tMask = null;
            ulong? ctMask = null;

            if (payload.ValueKind == JsonValueKind.Object)
            {
                if (payload.TryGetProperty("mode", out var m)) mode = m.GetString() ?? "ALL";
                if (payload.TryGetProperty("customMask", out var cm) && cm.ValueKind != JsonValueKind.Null)
                {
                    if (cm.ValueKind == JsonValueKind.String && ulong.TryParse(cm.GetString(), out ulong cmStrVal)) customMask = cmStrVal;
                    else if (cm.TryGetUInt64(out ulong cmVal)) customMask = cmVal;
                }
                if (payload.TryGetProperty("tMask", out var tm) && tm.ValueKind != JsonValueKind.Null)
                {
                    if (tm.ValueKind == JsonValueKind.String && ulong.TryParse(tm.GetString(), out ulong tmStrVal)) tMask = tmStrVal;
                    else if (tm.TryGetUInt64(out ulong tmVal)) tMask = tmVal;
                }
                if (payload.TryGetProperty("ctMask", out var ctm) && ctm.ValueKind != JsonValueKind.Null)
                {
                    if (ctm.ValueKind == JsonValueKind.String && ulong.TryParse(ctm.GetString(), out ulong ctmStrVal)) ctMask = ctmStrVal;
                    else if (ctm.TryGetUInt64(out ulong ctmVal)) ctMask = ctmVal;
                }
            }
            else if (payload.ValueKind == JsonValueKind.String)
            {
                string raw = payload.GetString() ?? "";
                string[] parts = raw.Split(':');
                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0])) mode = parts[0];
                if (parts.Length > 1 && ulong.TryParse(parts[1], out ulong m)) customMask = m;
                if (parts.Length > 2 && ulong.TryParse(parts[2], out ulong tm)) tMask = tm;
                if (parts.Length > 3 && ulong.TryParse(parts[3], out ulong ctm)) ctMask = ctm;
            }

            return (mode, customMask, tMask, ctMask);
        }
    }

    public class ExportVoiceCfgHandler : ICommandHandler
    {
        private readonly IDialogService _dialogService;
        private readonly AppSettings _settings;
        private readonly IFileSystemService _fileSystem;

        public ExportVoiceCfgHandler(IDialogService dialogService, AppSettings settings, IFileSystemService? fileSystem = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _fileSystem = fileSystem ?? new PhysicalFileSystemService();
        }

        public string CommandName => "EXPORT_VOICE_CFG";

        public void Execute(IpcRequest request)
        {
            var (mode, customMask, tMask, ctMask) = GenerateVoiceCfgHandler.ExtractVoiceParams(request.Payload);

            string cfgName = AppSettings.SanitizeConfigFileName(_settings.ConfigFileName);
            string defaultFileName = $"{cfgName}.cfg";

            string? exportPath = _dialogService.ShowSaveFileDialog(
                "Export CS2 Voice Channel Config",
                "CS2 Config File (*.cfg)|*.cfg|All Files (*.*)|*.*",
                defaultFileName
            );

            if (!string.IsNullOrEmpty(exportPath))
            {
                string content = VoiceBitmaskService.GenerateCS2Config(mode, customMask, tMask, ctMask, _settings);
                _fileSystem.WriteAllText(exportPath, content);
                _dialogService.ShowMessageBox(
                    $"CS2 Voice config exported successfully to:\n{exportPath}\n\nTo use in CS2, launch CS2 and type in console:\n+exec {Path.GetFileNameWithoutExtension(exportPath)}",
                    "DemoPulse - Export Complete"
                );
            }
        }
    }
}
