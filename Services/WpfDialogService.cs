using System.Windows;
using Microsoft.Win32;

namespace DemoPulse.Services
{
    public class WpfDialogService : IDialogService
    {
        public string? ShowOpenFileDialog(string title, string filter, string? defaultFileName = null, bool checkFileExists = true)
        {
            var dialog = new OpenFileDialog
            {
                Title = title,
                Filter = filter,
                CheckFileExists = checkFileExists
            };

            if (!string.IsNullOrEmpty(defaultFileName))
            {
                dialog.FileName = defaultFileName;
            }

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        public string? ShowSaveFileDialog(string title, string filter, string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Title = title,
                Filter = filter,
                FileName = defaultFileName
            };

            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return null;
        }

        public void ShowMessageBox(string message, string caption, bool isWarning = false)
        {
            MessageBox.Show(
                message,
                caption,
                MessageBoxButton.OK,
                isWarning ? MessageBoxImage.Warning : MessageBoxImage.Information
            );
        }
    }
}
