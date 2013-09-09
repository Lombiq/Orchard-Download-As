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
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Implementation;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;

namespace Lombiq.DownloadAs.Services
{
    [OrchardFeature("Lombiq.DownloadAs.Html")]
    public class HtmlFileBuildWorker : IFileBuildWorker
    {
        private readonly ISiteService _siteService;
        private readonly dynamic _shapeFactory;
        private readonly IShapeOutputGenerator _shapeOutputGenerator;

        private const string DisplayType = "File-html";

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


        public HtmlFileBuildWorker(
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IShapeOutputGenerator shapeOutputGenerator)
        {
            _siteService = siteService;
            _shapeFactory = shapeFactory;
            _shapeOutputGenerator = shapeOutputGenerator;

            T = NullLocalizer.Instance;
        }


        public Stream Build(IEnumerable<IContent> contents)
        {
            var contentManager = contents.First().ContentItem.ContentManager;

            var aliases = contents
                .Where(content => content.As<IAliasAspect>() != null)
                .Select(content => new
                {
                    Id = content.ContentItem.Id,
                    Path = content.As<IAliasAspect>().Path
                })
                .ToDictionary(alias => alias.Path);

            var siteUri = new Uri(_siteService.GetSiteSettings().BaseUrl);
            var contentShapes = contents.Select(content =>
                {
                    IShape contentShape = contentManager.BuildDisplay(content, DisplayType);

                    contentShape.Metadata.OnDisplayed(context =>
                        {
                            var doc = new HtmlDocument();
                            doc.LoadHtml(context.ShapeMetadata.ChildContent.ToHtmlString());

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

                            var stringBuilder = new StringBuilder();
                            using (var stringWriter = new StringWriter(stringBuilder))
                            {
                                doc.Save(stringWriter);
                            }
                            context.ShapeMetadata.ChildContent = new HtmlString(stringBuilder.ToString());
                        });

                    return contentShape;
                });

            var shape = _shapeFactory.DownloadAs_ContentsWrapper(ContentShapes: contentShapes);
            shape.Metadata.Alternates.Add("DownloadAs_ContentsWrapper__html");
            return _shapeOutputGenerator.GenerateOutput(shape);
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