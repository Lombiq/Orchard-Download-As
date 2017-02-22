using Orchard.Localization;

namespace Lombiq.DownloadAs.Models
{
    public interface IFileBuildWorkerDescriptor
    {
        string SupportedFileExtension { get; }
        LocalizedString DisplayName { get; }
    }
}
