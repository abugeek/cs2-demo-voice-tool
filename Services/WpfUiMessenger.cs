using System;
using System.Text.Json;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Wpf;
using DemoPulse.Interop.Contracts;

namespace DemoPulse.Services
{
    public class WpfUiMessenger : IUiMessenger
    {
        private WebView2? _webView;
        private Dispatcher? _dispatcher;

        public WpfUiMessenger()
        {
        }

        public WpfUiMessenger(WebView2 webView, Dispatcher dispatcher)
        {
            BindUi(webView, dispatcher);
        }

        public void BindUi(WebView2 webView, Dispatcher dispatcher)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void PostMessage(string message)
        {
            if (_webView == null || _dispatcher == null)
            {
                System.Diagnostics.Debug.WriteLine($"[WpfUiMessenger Warning] PostMessage dropped because UI is not bound: {message}");
                return;
            }

            if (_dispatcher.CheckAccess())
            {
                if (_webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.PostWebMessageAsString(message);
                }
            }
            else
            {
                _dispatcher.Invoke(() => PostMessage(message));
            }
        }

        public void PostJsonMessage(string json)
        {
            if (_webView == null || _dispatcher == null)
            {
                System.Diagnostics.Debug.WriteLine($"[WpfUiMessenger Warning] PostJsonMessage dropped because UI is not bound.");
                return;
            }

            if (_dispatcher.CheckAccess())
            {
                if (_webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.PostWebMessageAsJson(json);
                }
            }
            else
            {
                _dispatcher.Invoke(() => PostJsonMessage(json));
            }
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public const int MaxUnchunkedByteLength = 40000;
        public const int ChunkSize = 30000;

        public void SendResponse(IpcResponse response)
        {
            if (response == null) return;

            // Serialize directly to UTF-8 bytes to avoid intermediate Large Object Heap (LOH) string allocations
            byte[] utf8Bytes = JsonSerializer.SerializeToUtf8Bytes(response, JsonOptions);

            if (utf8Bytes.Length <= MaxUnchunkedByteLength)
            {
                string json = System.Text.Encoding.UTF8.GetString(utf8Bytes);
                PostJsonMessage(json);
            }
            else
            {
                SendChunkedResponse(utf8Bytes);
            }
        }

        private void SendChunkedResponse(byte[] utf8Bytes)
        {
            string chunkId = $"chunk_{Guid.NewGuid():N}";
            int totalChunks = (int)Math.Ceiling((double)utf8Bytes.Length / ChunkSize);

            for (int i = 0; i < totalChunks; i++)
            {
                int startIndex = i * ChunkSize;
                int length = Math.Min(ChunkSize, utf8Bytes.Length - startIndex);
                string part = System.Text.Encoding.UTF8.GetString(utf8Bytes, startIndex, length);

                var chunkResponse = new
                {
                    type = "IPC_CHUNK",
                    chunkId = chunkId,
                    index = i,
                    total = totalChunks,
                    data = part
                };

                string chunkJson = JsonSerializer.Serialize(chunkResponse, JsonOptions);
                PostJsonMessage(chunkJson);
            }
        }
    }
}
