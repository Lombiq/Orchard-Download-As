using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace Lombiq.DownloadAs.Handlers
{
    public class DownloadAsSettingsPartHandler : ContentHandler
    {
        public DownloadAsSettingsPartHandler(IRepository<DownloadAsSettingsPartRecord> repository)
        {
            Filters.Add(StorageFilter.For(repository));
            Filters.Add(new ActivatingFilter<DownloadAsSettingsPart>("Site"));
        }
    }
}