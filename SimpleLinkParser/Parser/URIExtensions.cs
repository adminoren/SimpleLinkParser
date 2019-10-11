using System;

namespace SimpleLinkParser.Parser
{
    public static class URIExtensions
    {
        public static string ToAbsolute(this Uri uri, string baseUrl)
        {
            var baseUri = new Uri(baseUrl);

            return uri.ToAbsolute(baseUri);
        }

        public static string ToAbsolute(this Uri uri, Uri baseUri)
        {
            var relative = uri.ToRelative();

            if (Uri.TryCreate(baseUri, relative, out var absolute))
            {
                return absolute.ToString();
            }

            return uri.IsAbsoluteUri ? uri.ToString() : null;
        }

        public static string ToRelative(this Uri uri)
        {
            return uri.IsAbsoluteUri ? uri.PathAndQuery : uri.OriginalString;
        }
    }
}
