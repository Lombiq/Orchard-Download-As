using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Lombiq.DownloadAs.Services;
using Orchard.ContentManagement;
using Orchard.Themes;

namespace Lombiq.DownloadAs.Controllers
{
    [Themed]
    public class TestController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IFileBuilder _fileBuilder;


        public TestController(IContentManager contentManager, IFileBuilder fileBuilder)
        {
            _contentManager = contentManager;
            _fileBuilder = fileBuilder;
        }


        public ViewResult Index()
        {
            var result = _fileBuilder.Build(_contentManager.Get(15), "html");
            using (var readStream = result.OpenRead())
            using (var stream = new MemoryStream())
            {
                readStream.CopyTo(stream);
                var z = Encoding.UTF8.GetString(stream.ToArray());
                var y = z;
            }

            return View();
        }
    }
}