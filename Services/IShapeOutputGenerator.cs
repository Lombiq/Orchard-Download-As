using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Orchard;
using Orchard.ContentManagement;

namespace Lombiq.DownloadAs.Services
{
    public interface IShapeOutputGenerator : IDependency
    {
        Stream GenerateOutput(dynamic shape);
    }
}
