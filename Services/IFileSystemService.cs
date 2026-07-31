using System.IO;

namespace DemoPulse.Services
{
    public interface IFileSystemService : IFileStreamProvider
    {
        bool FileExists(string path);
        bool DirectoryExists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string content);
        void CreateDirectory(string path);
    }
}
