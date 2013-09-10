using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace Lombiq.DownloadAs.Models
{
    public class DownloadAsSettingsPart : ContentPart<DownloadAsSettingsPartRecord>
    {
        public int CacheTimeoutMinutes
        {
            get { return Record.CacheTimeoutMinutes; }
            set { Record.CacheTimeoutMinutes = value; }
        }

        public TimeSpan CacheTimeout
        {
            get { return new TimeSpan(0, CacheTimeoutMinutes, 0); }
        }
    }


    public class DownloadAsSettingsPartRecord : ContentPartRecord
    {
        public virtual int CacheTimeoutMinutes { get; set; }


        public DownloadAsSettingsPartRecord()
        {
            CacheTimeoutMinutes = 10;
        }
    }
}