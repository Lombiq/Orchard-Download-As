using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using HtmlAgilityPack;
using Lombiq.DownloadAs.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;

namespace Lombiq.DownloadAs.Services
{
    [OrchardFeature("Lombiq.DownloadAs.Html")]
    public class HtmlFileBuildWorker : IFileBuildWorker
    {
        private readonly IWorkContextAccessor _wca;

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


        public HtmlFileBuildWorker(IWorkContextAccessor wca)
        {
            _wca = wca;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            var contentManager = contents.First().ContentItem.ContentManager;
            var workContext = _wca.GetContext();

            var controller = new DummyController(workContext.HttpContext.Request.RequestContext);

            var stringBuilder = new StringBuilder();
            using (var stringWriter = new StringWriter(stringBuilder))
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

                    var responseStream = new MemoryStream();
                    var responseStreamWriter = new StreamWriter(responseStream);

                    var aliases = contents
                        .Where(content => content.As<IAliasAspect>() != null)
                        .Select(content => new
                            {
                                Id = content.ContentItem.Id,
                                Path = content.As<IAliasAspect>().Path
                            })
                        .ToDictionary(alias => alias.Path);

                    foreach (var content in contents)
                    {
                        stringWriter.Write("<a name=\"content-item-top-" + content.ContentItem.Id + "\"></a>");

                        new ShapePartialResult(controller, contentManager.BuildDisplay(content, "File-html"))
                            .ExecuteResult(controller.ControllerContext);

                        var html = stringBuilder.ToString();

                        #region Adjust internal links
                        var siteUri = new Uri(workContext.CurrentSite.BaseUrl);
                        var urls = new HashSet<string>();
                        var doc = new HtmlDocument();
                        doc.LoadHtml(html);

                        var links = doc.DocumentNode.SelectNodes("//a[@href]");
                        if (links != null) // See: https://htmlagilitypack.codeplex.com/workitem/29175
                        {
                            var aliasAspect = content.As<IAliasAspect>();
                            if (aliasAspect != null)
                            {
                                var itemUri = new Uri(siteUri, aliasAspect.Path);
                                foreach (var link in links)
                                {
                                    var href = link.GetAttributeValue("href", null);
                                    if (href != null)
                                    {
                                        Uri uri = null;
                                        if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                                        {
                                            uri = new Uri(itemUri, href);
                                        }
                                        else
                                        {
                                            var linkUri = new Uri(href);
                                            if (linkUri.Host == siteUri.Host) uri = linkUri;
                                        }

                                        if (uri != null) // The uri is an internal one
                                        {
                                            var alias = uri.LocalPath.TrimStart('/');
                                            if (aliases.ContainsKey(alias))
                                            {
                                                link.SetAttributeValue("href", "#content-item-top-" + aliases[alias].Id);
                                            }
                                            else
                                            {
                                                link.SetAttributeValue("href", uri.ToString());
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        doc.Save(responseStreamWriter);
                        #endregion

                        stringBuilder.Clear();
                    }

                    responseStreamWriter.Flush();


                    newResponse.Flush();
                    htmlTextWriter.Flush();
                    stringWriter.Flush();

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