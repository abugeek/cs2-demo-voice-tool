using System;
using System.Text.Json;
using DemoPulse.Interop.Contracts;
using DemoPulse.Models;
using DemoPulse.Services;

namespace DemoPulse.Interop.Handlers
{
    public class SelectFileHandler : ICommandHandler
    {
        private readonly IDialogService _dialogService;
        private readonly IDemoService _demoService;

        public SelectFileHandler(IDialogService dialogService, IDemoService demoService)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _demoService = demoService ?? throw new ArgumentNullException(nameof(demoService));
        }

        public string CommandName => "SELECT_FILE";

        public void Execute(IpcRequest request)
        {
            string? file = _dialogService.ShowOpenFileDialog(
                "Select Counter-Strike 2 Match Demo",
                "CS2 Demo Files (*.dem)|*.dem|All Files (*.*)|*.*"
            );

            if (!string.IsNullOrEmpty(file))
            {
                _ = _demoService.LoadDemoPathAsync(file, request.Id);
            }
        }
    }

    public class ParseDemoHandler : ICommandHandler
    {
        private readonly IDemoService _demoService;

        public ParseDemoHandler(IDemoService demoService)
        {
            _demoService = demoService ?? throw new ArgumentNullException(nameof(demoService));
        }

        public string CommandName => "PARSE_DEMO";

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

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _ = _demoService.LoadDemoPathAsync(filePath, request.Id);
            }
        }
    }

    public class LaunchCs2Handler : ICommandHandler
    {
        private readonly IDialogService _dialogService;
        private readonly AppSettings _settings;
        private readonly IDemoService? _demoService;

        public LaunchCs2Handler(IDialogService dialogService, AppSettings settings, IDemoService? demoService = null)
        {
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _demoService = demoService;
        }

        public string CommandName => "LAUNCH_CS2";

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public void Execute(IpcRequest request)
        {
            try
            {
                string cfgName = string.IsNullOrWhiteSpace(_settings.ConfigFileName) ? "demopulse" : _settings.ConfigFileName;
                string filePath = "";
                ulong? tMask = null;
                ulong? ctMask = null;

                if (request.Payload.ValueKind == JsonValueKind.Object)
                {
                    if (request.Payload.TryGetProperty("filePath", out var pathProp))
                        filePath = pathProp.GetString() ?? "";
                    
                    if (request.Payload.TryGetProperty("tMask", out var tm) && tm.ValueKind != JsonValueKind.Null)
                    {
                        if (tm.ValueKind == JsonValueKind.String && ulong.TryParse(tm.GetString(), out ulong tmVal)) tMask = tmVal;
                        else if (tm.TryGetUInt64(out ulong tmValNum)) tMask = tmValNum;
                    }
                    if (request.Payload.TryGetProperty("ctMask", out var ctm) && ctm.ValueKind != JsonValueKind.Null)
                    {
                        if (ctm.ValueKind == JsonValueKind.String && ulong.TryParse(ctm.GetString(), out ulong ctmVal)) ctMask = ctmVal;
                        else if (ctm.TryGetUInt64(out ulong ctmValNum)) ctMask = ctmValNum;
                    }
                }
                else if (request.Payload.ValueKind == JsonValueKind.String)
                {
                    filePath = request.Payload.GetString() ?? "";
                }

                // Fallback to active parsed match data if payload masks were not supplied
                if (tMask == null && _demoService?.CurrentMatchData != null)
                {
                    tMask = _demoService.CurrentMatchData.VoiceConfig.TSideBitmask;
                    ctMask = _demoService.CurrentMatchData.VoiceConfig.CtSideBitmask;
                }

                // Ensure cfg file is saved to CS2 cfg directory prior to launch so +exec works
                try
                {
                    string cfgContent = VoiceBitmaskService.GenerateCS2Config("ALL", null, tMask, ctMask, _settings);
                    GenerateVoiceCfgHandler.AutoSaveConfigToCs2Folder(cfgContent, _settings, out _);
                }
                catch { }

                string demoToPlay = "watch_current";

                if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath))
                {
                    string csgoDir = _settings.GetCs2GameFolder();
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string fileNameNoExt = System.IO.Path.GetFileNameWithoutExtension(filePath);

                    if (!string.IsNullOrWhiteSpace(csgoDir) && System.IO.Directory.Exists(csgoDir))
                    {
                        string targetInCsgo = System.IO.Path.Combine(csgoDir, fileName);
                        bool isAlreadyInCsgo = filePath.StartsWith(csgoDir, StringComparison.OrdinalIgnoreCase);

                        if (!isAlreadyInCsgo)
                        {
                            try
                            {
                                System.IO.File.Copy(filePath, targetInCsgo, overwrite: true);
                                demoToPlay = fileNameNoExt;
                            }
                            catch
                            {
                                demoToPlay = $"\"{filePath}\"";
                            }
                        }
                        else
                        {
                            demoToPlay = fileNameNoExt;
                        }
                    }
                    else
                    {
                        demoToPlay = $"\"{filePath}\"";
                    }
                }

                string encodedCfgName = Uri.EscapeDataString(cfgName);
                string encodedDemoToPlay = Uri.EscapeDataString(demoToPlay);

                var cs2Processes = System.Diagnostics.Process.GetProcessesByName("cs2");
                if (cs2Processes.Length > 0)
                {
                    var proc = cs2Processes[0];
                    if (proc.MainWindowHandle != IntPtr.Zero)
                    {
                        ShowWindow(proc.MainWindowHandle, SW_RESTORE);
                        SetForegroundWindow(proc.MainWindowHandle);
                    }

                    string consoleCmd = $"exec {cfgName}; playdemo {demoToPlay}";
                    SetClipboard(consoleCmd);

                    Action showDialogAction = () =>
                    {
                        _dialogService.ShowMessageBox(
                            $"CS2 is already running!\n\nDemoPulse has focused CS2 and copied the play command to your clipboard:\n\n{consoleCmd}\n\nPress '~' in CS2 console and hit Ctrl+V + Enter!",
                            "CS2 Focused & Command Copied",
                            isWarning: false
                        );
                    };

                    if (System.Windows.Application.Current?.Dispatcher != null)
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(showDialogAction);
                    }
                    else
                    {
                        showDialogAction();
                    }
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = $"steam://rungameid/730//+exec%20{encodedCfgName}%20+playdemo%20{encodedDemoToPlay}",
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Action showErrorAction = () =>
                {
                    _dialogService.ShowMessageBox(
                        "Could not launch CS2 via Steam: " + ex.Message,
                        "DemoPulse Error",
                        isWarning: true
                    );
                };

                if (System.Windows.Application.Current?.Dispatcher != null)
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(showErrorAction);
                }
                else
                {
                    showErrorAction();
                }
            }
        }

        private static void SetClipboard(string text)
        {
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    System.Windows.Clipboard.SetText(text);
                }
                catch { }
            });
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}
