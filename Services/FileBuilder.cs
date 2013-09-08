using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Lombiq.DownloadAs.Models;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Services;
using Piedone.HelpfulLibraries.Utilities;

namespace Lombiq.DownloadAs.Services
{
    public class FileBuilder : IFileBuilder
    {
        private readonly IEnumerable<IFileBuildWorker> _fileBuilderWorkers;
        private readonly IStorageProvider _storageProvider;
        private readonly IClock _clock;

        private const string CacheFolderPath = "_LombiqModules/DownloadAs/CacheFiles/";
        private static readonly TimeSpan CacheDuration = new TimeSpan(0, 0, 1); // 1s only for testing


        public FileBuilder(
            IEnumerable<IFileBuildWorker> fileBuilderWorkers,
            IStorageProvider storageProvider,
            IClock clock)
        {
            _fileBuilderWorkers = fileBuilderWorkers;
            _storageProvider = storageProvider;
            _clock = clock;
        }


        public IFileResult Build(IContent content, string extension)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (string.IsNullOrEmpty(extension)) throw new ArgumentNullException("extensions");

            var filePath = CacheFolderPath + content.ContentItem + "." + extension;
            var mimeType = MimeAssistant.GetMimeType(filePath);

            if (_storageProvider.FileExists(filePath))
            {
                var file = _storageProvider.GetFile(filePath);
                if (file.GetLastUpdated().ToUniversalTime().Add(CacheDuration) >= _clock.UtcNow)
                {
                    return new FileResult(() => file.OpenRead(), mimeType);
                }
                else
                {
                    _storageProvider.DeleteFile(filePath);
                }
            }

            var worker = _fileBuilderWorkers.Where(w => string.Compare(w.Descriptor.SupportedFileExtension, extension, StringComparison.OrdinalIgnoreCase) == 0).LastOrDefault();

            if (worker == null) throw new NotSupportedException("There is no worker for building a file of type " + extension + ".");

            var contents = new[] { content };

            var stream = worker.Build(contents);

            var newFile = _storageProvider.CreateFile(filePath);
            using (var writeStream = newFile.OpenWrite())
            {
                stream.CopyTo(writeStream);
            }

            return new FileResult(() =>
            {
                stream.Position = 0;
                return stream;
            }, mimeType);
        }


        private class FileResult : IFileResult
        {
            private Func<Stream> _streamFactory;

            public string MimeType { get; private set; }


            public FileResult(Func<Stream> streamFactory, string mimeType)
            {
                _streamFactory = streamFactory;
                MimeType = mimeType;
            }


            public Stream OpenRead()
            {
                return _streamFactory();
            }
        }
    }
}