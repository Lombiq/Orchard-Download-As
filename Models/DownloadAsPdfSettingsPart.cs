using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Extensions;

namespace Lombiq.DownloadAs.Models
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class DownloadAsPdfSettingsPart : ContentPart<DownloadAsPdfSettingsPartRecord>
    {
        [StringLength(2048)]
        public string CloudConvertApiKey
        {
            get { return Record.CloudConvertApiKey; }
            set { Record.CloudConvertApiKey = value; }
        }
    }


    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class DownloadAsPdfSettingsPartRecord : ContentPartRecord
    {
        public virtual string CloudConvertApiKey { get; set; }
    }
}