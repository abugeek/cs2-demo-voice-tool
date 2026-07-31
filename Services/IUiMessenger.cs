using DemoPulse.Interop.Contracts;

namespace DemoPulse.Services
{
    /// <summary>
    /// Abstraction for posting IPC web messages and sending structured responses to the UI frontend.
    /// </summary>
    public interface IUiMessenger
    {
        /// <summary>
        /// Posts a raw string or JSON message to the WebView2 UI frontend.
        /// </summary>
        /// <param name="message">The raw message payload string.</param>
        void PostMessage(string message);

        /// <summary>
        /// Sends a strongly typed IPC response to the UI frontend with automatic chunking for large payloads.
        /// </summary>
        /// <param name="response">The IPC response contract object.</param>
        void SendResponse(IpcResponse response);
    }
}
