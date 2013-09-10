using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.Data.Migration;
using Orchard.Environment.Extensions;

namespace Lombiq.DownloadAs.Migrations
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class PdfMigrations : DataMigrationImpl
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(DownloadAsPdfSettingsPartRecord).Name,
                table => table
                    .ContentPartRecord()
                    .Column<string>("CloudConvertApiKey", column => column.WithLength(2048))
                );


            return 1;
        }
    }
}