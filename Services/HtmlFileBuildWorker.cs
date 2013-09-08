using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc;

namespace Lombiq.DownloadAs.Services
{
    public class HtmlFileBuildWorker : IFileBuildWorker
    {
        private readonly IHttpContextAccessor _hca;

        private IFileBuildWorkerDescriptor _descriptor;
        public IFileBuildWorkerDescriptor Descriptor
        {
            get
            {
                if (_descriptor != null) return _descriptor;

                _descriptor = new FileBuildWorkerDescriptor
                {
                    SupportedFileExtension = "html",
                    DisplayName = T("Html")
                };

                return _descriptor;
            }
        }

        public Localizer T { get; set; }


        public HtmlFileBuildWorker(IHttpContextAccessor hca)
        {
            _hca = hca;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            var contentManager = contents.First().ContentItem.ContentManager;

            var controller = new DummyController(_hca.Current().Request.RequestContext);


            using (var stream = new MemoryStream())
            using (var stringWriter = new StreamWriter(stream))
            using (var htmlTextWriter = new HtmlTextWriter(stringWriter))
            {
                var originalContext = controller.ControllerContext.HttpContext;

                try
                {
                    // Get the Request and User objects from the current, unchanged context
                    var currentRequest = HttpContext.Current.ApplicationInstance.Context.Request;
                    var currentUser = HttpContext.Current.ApplicationInstance.Context.User;

                    // Create our new HttpResponse object containing our HtmlTextWriter
                    var newResponse = new HttpResponse(htmlTextWriter);

                    // Create a new HttpContext object using our new Response object and the existing Request and User objects
                    var newContext = new HttpContextWrapper(
                                new HttpContext(currentRequest, newResponse)
                                {
                                    User = currentUser
                                });

                    // Swap in our new HttpContext object - output from this controller is now going to our HtmlTextWriter object
                    controller.ControllerContext.HttpContext = newContext;

                    foreach (var content in contents)
                    {
                        new ShapePartialResult(controller, contentManager.BuildDisplay(content))
                            .ExecuteResult(controller.ControllerContext); 
                    }


                    newResponse.Flush();
                    htmlTextWriter.Flush();
                    stringWriter.Flush();

                    var responseStream = new MemoryStream(); // So everything else here can be disposed
                    stream.Position = 0;
                    stream.CopyTo(responseStream);
                    responseStream.Position = 0;
                    return responseStream;
                }
                finally
                {
                    // Setting context back to original so nothing gets messed up
                    controller.ControllerContext.HttpContext = originalContext;
                }
            }
        }


        private class DummyController : ControllerBase
        {
            public DummyController(RequestContext requestContext)
            {
                ControllerContext = new ControllerContext(requestContext, this);
            }


            protected override void ExecuteCore()
            {
                throw new NotImplementedException();
            }
        }
    }
}