using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DemoPulse.Services.Providers
{
    public class MockFileSystemService : IFileSystemService
    {
        public Dictionary<string, byte[]> Files { get; } = new(System.StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Directories { get; } = new(System.StringComparer.OrdinalIgnoreCase);

        public bool FileExists(string path) => Files.ContainsKey(path);

        public bool DirectoryExists(string path) => Directories.Contains(path);

        public string ReadAllText(string path)
        {
            if (Files.TryGetValue(path, out var bytes))
                return Encoding.UTF8.GetString(bytes);
            throw new FileNotFoundException($"Mock file not found: {path}");
        }

        public void WriteAllText(string path, string content)
        {
            Files[path] = Encoding.UTF8.GetBytes(content);
        }

        public void CreateDirectory(string path)
        {
            Directories.Add(path);
        }

        public Stream OpenReadStream(string filePath)
        {
            if (Files.TryGetValue(filePath, out var bytes))
                return new MemoryStream(bytes);
            throw new FileNotFoundException($"Mock file not found: {filePath}");
        }
    }
}
