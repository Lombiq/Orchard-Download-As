using System;

namespace Lombiq.DownloadAs
{
    internal static class UrlHelper
    {
        public static bool UrlIsInternal(Uri itemUri, string url, out Uri uri)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                uri = new Uri(itemUri, url);
                return true;
            }
            else if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                uri = new Uri(url);
                return uri.Host == itemUri.Host;
            }
            else
            {
                uri = null;
                return false;
            }
        }
    }
}