using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lombiq.DownloadAs.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Lombiq.DownloadAs.Services
{
    public interface IFileBuilder : IDependency
    {
        IFileResult Build(IContent content, string extension);
        IFileResult BuildRecursive(IContent content, string extension);
    }
}
