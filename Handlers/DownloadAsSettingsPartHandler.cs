using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement.Handlers;

namespace Lombiq.DownloadAs.Handlers
{
    public class DownloadAsSettingsPartHandler : ContentHandler
    {
        public DownloadAsSettingsPartHandler()
        {
            Filters.Add(new ActivatingFilter<DownloadAsSettingsPart>("Site"));
        }
    }
}