using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment.Extensions;

namespace Lombiq.DownloadAs.Drivers
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class DownloadAsPdfSettingsPartDriver : ContentPartDriver<DownloadAsPdfSettingsPart>
    {
        protected override DriverResult Editor(DownloadAsPdfSettingsPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_DownloadAsPdfSettings_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: "Parts.DownloadAsPdfSettings",
                    Model: part,
                    Prefix: Prefix));
        }

        protected override DriverResult Editor(DownloadAsPdfSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }

        protected override void Exporting(DownloadAsPdfSettingsPart part, ExportContentContext context)
        {
            var element = context.Element(part.PartDefinition.Name);

            element.SetAttributeValue("CloudConvertApiKey", part.CloudConvertApiKey);
        }

        protected override void Importing(DownloadAsPdfSettingsPart part, ImportContentContext context)
        {
            var partName = part.PartDefinition.Name;

            context.ImportAttribute(partName, "CloudConvertApiKey", value => part.CloudConvertApiKey = value);
        }
    }
}