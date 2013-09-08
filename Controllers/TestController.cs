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
    public class TestController : Controller
    {
        private readonly IContentManager _contentManager;
        private readonly IFileBuilder _fileBuilder;


        public TestController(IContentManager contentManager, IFileBuilder fileBuilder)
        {
            _contentManager = contentManager;
            _fileBuilder = fileBuilder;
        }


        public FileResult Index()
        {
            var result = _fileBuilder.BuildRecursive(_contentManager.Get(16), "html");
            return File(result.OpenRead(), result.MimeType, "testfile.html");
        }
    }
}