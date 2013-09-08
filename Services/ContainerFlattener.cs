using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;

namespace Lombiq.DownloadAs.Services
{
    public class ContainerFlattener : IContainerFlattener
    {
        public IEnumerable<IContent> Flatten(IContent container)
        {
            return FlattenOneLevel(container);
        }


        private IEnumerable<IContent> FlattenOneLevel(IContent container)
        {
            var items = new List<IContent>();

            items.Add(container);

            var containedItems = container.ContentItem.ContentManager
                .Query()
                .Where<CommonPartRecord>(record => record.Container.Id == container.ContentItem.Id)
                .List<IContent>();
            foreach (var item in containedItems)
            {
                items.AddRange(FlattenOneLevel(item));
            }

            return items;
        }
    }
}