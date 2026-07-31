using System.IO;

namespace DemoPulse.Services.Providers
{
    public class PhysicalFileSystemService : IFileSystemService
    {
        public bool FileExists(string path) => File.Exists(path);

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public string ReadAllText(string path) => File.ReadAllText(path);

        public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public Stream OpenReadStream(string filePath)
        {
            return new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1024 * 1024,
                options: FileOptions.SequentialScan);
        }
    }
}
