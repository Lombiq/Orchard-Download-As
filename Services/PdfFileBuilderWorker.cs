using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Settings;
using RestSharp;

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
            using (var htmlStream = _htmlGenerator.GenerateHtml(contents, Descriptor.SupportedFileExtension))
            using (var tmpStream = new MemoryStream())
            {
                htmlStream.CopyTo(tmpStream);

                var apiKey = _siteService.GetSiteSettings().As<DownloadAsPdfSettingsPart>().CloudConvertApiKey;

                var client = new RestClient("https://api.cloudconvert.org/");
                var request = new RestRequest("process");
                request.AddParameter("apikey", apiKey);
                request.AddParameter("inputformat", "html");
                request.AddParameter("outputformat", "pdf");
                request.AddParameter("converter", "unoconv");
                var response = client.Post<InitResponse>(request);
                var processUrl = response.Data.Url;
                if (response.StatusCode != HttpStatusCode.OK) throw new OrchardException(T("PDF generation failed as the CloudConvert API returned an error when trying to open the converion process. Status code: {0}. CloudConvert error message: {1}.", response.StatusCode, response.Data.Error));

                var processClient = new RestClient("https:" + processUrl);

                var uploadRequest = new RestRequest();
                uploadRequest.AddFile("file", tmpStream.ToArray(), "output.html");
                uploadRequest.AddParameter("input", "upload");
                uploadRequest.AddParameter("format", "pdf");
                var uploadResponse = processClient.Post(uploadRequest);
                if (uploadResponse.StatusCode != HttpStatusCode.OK) throw new OrchardException(T("PDF generation failed as the CloudConvert API returned an error when trying to upload the HTML file. Status code: {0}.", uploadResponse.StatusCode));

                var processResponse = processClient.Get<ProcessResponse>(new RestRequest());
                var tryCount = 0;
                while (processResponse.Data.Step != "finished" && tryCount < 20)
                {
                    Thread.Sleep(2000); // Yes, doing this like this is bad. No better idea yet.
                    processResponse = processClient.Get<ProcessResponse>(new RestRequest());
                    tryCount++;
                }
                if (tryCount == 20) throw new OrchardException(T("PDF generation failed as CloudConvert didn't finish the conversion after 40s."));


                using (var wc = new WebClient())
                {
                    var fileBytes = wc.DownloadData("https:" + processResponse.Data.Output.Url);
                    return new MemoryStream(fileBytes);
                }
            }
        }


        public class InitResponse
        {
            public string Error { get; set; }
            public string Url { get; set; }
        }

        public class ProcessResponse
        {
            public string Step { get; set; }
            public ProcessOutput Output { get; set; }
        }

        public class ProcessOutput
        {
            public string Url { get; set; }
        }
    }
}