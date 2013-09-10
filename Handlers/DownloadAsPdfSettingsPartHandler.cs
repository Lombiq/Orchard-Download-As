using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Environment.Extensions;

namespace Lombiq.DownloadAs.Handlers
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class DownloadAsPdfSettingsPartHandler : ContentHandler
    {
        public DownloadAsPdfSettingsPartHandler(IRepository<DownloadAsPdfSettingsPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<DownloadAsPdfSettingsPart>("Site"));
        }
    }
}