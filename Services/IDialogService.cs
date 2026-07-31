namespace DemoPulse.Services
{
    /// <summary>
    /// Service abstraction for native OS dialog interactions (file open, file save, message box).
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays an open file dialog and returns the selected path.
        /// </summary>
        string? ShowOpenFileDialog(string title, string filter, string? defaultFileName = null, bool checkFileExists = true);

        /// <summary>
        /// Displays a save file dialog and returns the target export path.
        /// </summary>
        string? ShowSaveFileDialog(string title, string filter, string defaultFileName);

        /// <summary>
        /// Displays a native message box alert or warning to the user.
        /// </summary>
        void ShowMessageBox(string message, string caption, bool isWarning = false);
    }
}
