using System.Collections.Generic;
using System.IO;
using System.Text;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Lombiq.DownloadAs.Services
{
    [OrchardFeature("Lombiq.DownloadAs.Html")]
    public class HtmlFileBuilderWorker : IFileBuilderWorker
    {
        private readonly IFlattenedHtmlGenerator _htmlGenerator;

        private IFileBuildWorkerDescriptor _descriptor;
        public IFileBuildWorkerDescriptor Descriptor
        {
            get
            {
                if (_descriptor != null) return _descriptor;

                _descriptor = new FileBuildWorkerDescriptor
                {
                    SupportedFileExtension = "html",
                    DisplayName = T("HTML")
                };

                return _descriptor;
            }
        }

        public Localizer T { get; set; }


        public HtmlFileBuilderWorker(IFlattenedHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_htmlGenerator.GenerateHtml(contents, Descriptor.SupportedFileExtension)));
        }
    }
}