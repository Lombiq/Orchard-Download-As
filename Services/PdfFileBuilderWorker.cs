using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Settings;

namespace Lombiq.DownloadAs.Services
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class PdfFileBuilderWorker : IFileBuildWorker
    {
        private readonly IFlattenedHtmlGenerator _htmlGenerator;
        private readonly ISiteService _siteService;

        private IFileBuildWorkerDescriptor _descriptor;
        public IFileBuildWorkerDescriptor Descriptor
        {
            get
            {
                if (_descriptor != null) return _descriptor;

                _descriptor = new FileBuildWorkerDescriptor
                {
                    SupportedFileExtension = "pdf",
                    DisplayName = T("PDF")
                };

                return _descriptor;
            }
        }

        public Localizer T { get; set; }


        public PdfFileBuilderWorker(
            IFlattenedHtmlGenerator htmlGenerator,
            ISiteService siteService)
        {  
            _htmlGenerator = htmlGenerator;
            _siteService = siteService;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            var apiKey = _siteService.GetSiteSettings().As<DownloadAsPdfSettingsPart>().CloudConvertApiKey;
            return _htmlGenerator.GenerateHtml(contents, Descriptor.SupportedFileExtension);
        }
    }
}