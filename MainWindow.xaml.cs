using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using DemoPulse.Models;
using DemoPulse.Services;
using DemoPulse.Interop;

namespace DemoPulse
{
    public partial class MainWindow : Window
    {
        private readonly AppSettings _settings;
        private readonly IUiMessenger _messenger;
        private readonly IDialogService _dialogService;
        private readonly IDemoService _demoService;
        private readonly WebViewMessageRouter _router;

        public MainWindow(
            AppSettings settings,
            IUiMessenger messenger,
            IDialogService dialogService,
            IDemoService demoService,
            WebViewMessageRouter router)
        {
            InitializeComponent();

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _demoService = demoService ?? throw new ArgumentNullException(nameof(demoService));
            _router = router ?? throw new ArgumentNullException(nameof(router));

            if (_messenger is WpfUiMessenger wpfMessenger)
            {
                wpfMessenger.BindUi(webView, Dispatcher);
            }

            InitializeAsync();
        }

        public MainWindow() : this(
            App.Services.GetRequiredService<AppSettings>(),
            App.Services.GetRequiredService<IUiMessenger>(),
            App.Services.GetRequiredService<IDialogService>(),
            App.Services.GetRequiredService<IDemoService>(),
            App.Services.GetRequiredService<WebViewMessageRouter>())
        {
        }

        private async void InitializeAsync()
        {
            try
            {
                await webView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to initialize WebView2. Please ensure the WebView2 Runtime is installed.\n\nError: " + ex.Message, "WebView2 Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string uiFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui");
            if (Directory.Exists(uiFolder))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "demopulse.local",
                    uiFolder,
                    CoreWebView2HostResourceAccessKind.Allow
                );
                webView.Source = new Uri("https://demopulse.local/index.html");
            }
            else
            {
                string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui", "index.html");
                if (File.Exists(htmlPath))
                    webView.Source = new Uri(htmlPath);
            }

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webView.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;

            // Support opening demo via command-line argument (e.g. file association)
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                string demoPath = args[1].Trim('"');
                if (IsValidDemoFile(demoPath))
                {
                    webView.CoreWebView2.NavigationCompleted += async (s, e) =>
                    {
                        await _demoService.LoadDemoPathAsync(demoPath);
                    };
                }
            }
        }

        private static bool IsValidDemoFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return File.Exists(path) && !Directory.Exists(path) && path.EndsWith(".dem", StringComparison.OrdinalIgnoreCase);
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string msg = e.TryGetWebMessageAsString();
                _router.HandleMessage(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CoreWebView2 Error] WebMessageReceived error: {ex.Message}");
            }
        }

        private void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2 ProcessFailed] Kind: {e.ProcessFailedKind}, ExitCode: {e.ExitCode}, Reason: {e.Reason}");

            try
            {
                // Auto-recover for render process exit/unresponsiveness or GPU crash
                if (e.ProcessFailedKind == CoreWebView2ProcessFailedKind.RenderProcessExited ||
                    e.ProcessFailedKind == CoreWebView2ProcessFailedKind.RenderProcessUnresponsive ||
                    e.ProcessFailedKind == CoreWebView2ProcessFailedKind.GpuProcessExited ||
                    e.ProcessFailedKind == CoreWebView2ProcessFailedKind.FrameRenderProcessExited)
                {
                    System.Diagnostics.Debug.WriteLine($"[WebView2 Self-Healing] Auto-recovering WebView2 runtime following {e.ProcessFailedKind}.");

                    if (_messenger is WpfUiMessenger wpfMessenger)
                    {
                        wpfMessenger.BindUi(webView, Dispatcher);
                    }

                    webView.Reload();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[WebView2 ProcessFailed Recovery Exception] {ex.Message}");
            }
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0)
                {
                    string path = files[0];
                    if (IsValidDemoFile(path) || Directory.Exists(path))
                    {
                        e.Effects = DragDropEffects.Copy;
                        e.Handled = true;
                        return;
                    }
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0 && IsValidDemoFile(files[0]))
                {
                    string path = files[0];
                    _ = _demoService.LoadDemoPathAsync(path);
                }
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMax_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}