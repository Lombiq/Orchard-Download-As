using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using Orchard.ContentManagement;

namespace Lombiq.DownloadAs.Services
{
    public interface IContainerFlattener : IDependency
    {
        IEnumerable<IContent> Flatten(IContent container);
    }
}
