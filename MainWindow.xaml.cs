using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace DemoPulse
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui", "index.html");
            if (File.Exists(htmlPath))
            {
                webView.Source = new Uri(htmlPath);
            }

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && args[1].EndsWith(".dem", StringComparison.OrdinalIgnoreCase))
            {
                string demoPath = args[1].Trim('"');
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    webView.CoreWebView2.PostWebMessageAsString($"OPEN_DEMO:{demoPath}");
                };
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string msg = e.TryGetWebMessageAsString();
            if (msg == "LAUNCH_CS2")
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = "/C start steam://rungameid/730//+exec%20demo%20+playdemo%20watch_current",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void BtnMax_Click(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}