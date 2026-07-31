using DemoPulse.Interop.Contracts;

namespace DemoPulse.Interop
{
    /// <summary>
    /// Contract interface for executing dispatched IPC commands received from the WebView2 UI.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Gets the unique command identifier key.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Executes the command using the specified request payload envelope.
        /// </summary>
        /// <param name="request">The incoming IPC request contract object.</param>
        void Execute(IpcRequest request);
    }
}
