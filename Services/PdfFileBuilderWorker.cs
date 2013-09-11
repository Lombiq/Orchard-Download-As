using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;

namespace Lombiq.DownloadAs.Services
{
    [OrchardFeature("Lombiq.DownloadAs.Pdf")]
    public class PdfFileBuilderWorker : IFileBuildWorker
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
                    SupportedFileExtension = "pdf",
                    DisplayName = T("PDF")
                };

                return _descriptor;
            }
        }

        public Localizer T { get; set; }


        public PdfFileBuilderWorker(IFlattenedHtmlGenerator htmlGenerator)
        {
            _htmlGenerator = htmlGenerator;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            string html;
            using (var htmlStream = _htmlGenerator.GenerateHtml(contents, Descriptor.SupportedFileExtension))
            using (var streamReader = new StreamReader(htmlStream))
            {
                html = streamReader.ReadToEnd();
                html = html.Replace("<hr>", ""); // hr tags cause NE on worker.Parse()
            }

            using (var htmlReader = new StringReader(html))
            using (var pdfStream = new MemoryStream())
            using (var document = new Document(PageSize.A4, 80, 50, 30, 65))
            using (var writer = PdfWriter.GetInstance(document, pdfStream))
            using (var worker = new HTMLWorker(document))
            {

                document.Open();
                worker.StartDocument();

                worker.Parse(htmlReader);

                worker.EndDocument();
                worker.Close();
                document.Close();

                // This copying is needed because the document should be closed. This in turn however also disposes pdfStream.
                return new MemoryStream(pdfStream.ToArray());
            }
        }
    }
}