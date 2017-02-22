using System.IO;

namespace Lombiq.DownloadAs.Models
{
    public interface IFileResult
    {
        string MimeType { get; }
        Stream OpenRead();
    }
}
