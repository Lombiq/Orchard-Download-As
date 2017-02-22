﻿using System.Collections.Generic;
using System.IO;
using Lombiq.DownloadAs.Models;
using Orchard;
using Orchard.ContentManagement;

namespace Lombiq.DownloadAs.Services
{
    public interface IFileBuilderWorker : IDependency
    {
        IFileBuildWorkerDescriptor Descriptor { get; }
        Stream Build(IEnumerable<IContent> contents);
    }
}
