using System.IO;

namespace DemoPulse.Services
{
    public interface IFileStreamProvider
    {
        Stream OpenReadStream(string filePath);
    }
}
