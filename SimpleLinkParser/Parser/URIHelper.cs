using System;

namespace SimpleLinkParser.Parser
{
    public static class URIHelper
    {
        public static string GetAbsoluteLink(string rawLink, string pageUrl)
        {
            if (string.IsNullOrEmpty(rawLink))
            {
                return null;
            }

            if (Uri.IsWellFormedUriString(rawLink, UriKind.Absolute))
            {
                return new Uri(rawLink).ToString();
            }

            if (Uri.IsWellFormedUriString(rawLink, UriKind.Relative))
            {
                var baseUri = new Uri(pageUrl).GetLeftPart(UriPartial.Authority);
                if (baseUri != null)
                {
                    return new Uri(rawLink, UriKind.Relative).ToAbsolute(baseUri);
                }
            }

            return null;
        }

        public static bool IsLinkLocalForDomain(string link, string srcLink)
        {
            var linkDomain = GetSecondLevelDomain(link);
            var srcLinkdomain = GetSecondLevelDomain(srcLink);

            return (!string.IsNullOrEmpty(linkDomain) && !string.IsNullOrEmpty(srcLinkdomain) && linkDomain.Equals(srcLinkdomain, StringComparison.InvariantCultureIgnoreCase));
        }

        public static string GetSecondLevelDomain(string link)
        {
            var uri = new Uri(link);
            var splitHostName = uri.Host.Split('.');

            if (splitHostName.Length >= 2)
            {
                return splitHostName[splitHostName.Length - 2] + "." + splitHostName[splitHostName.Length - 1];
            }

            return null;
        }
    }
}
