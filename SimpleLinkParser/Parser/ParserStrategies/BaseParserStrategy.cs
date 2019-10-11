using HtmlAgilityPack;
using System.Linq;

namespace SimpleLinkParser.Parser.ParserStrategies
{
    public class BaseParserStrategy : ILinkParserStrategy
    {
        public string Name => nameof(BaseParserStrategy);

        ParseResult ILinkParserStrategy.GetLinks(string page, string sourceUrl)
        {
            if (string.IsNullOrWhiteSpace(page))
            {
                return new ParseResult(new string[0], new string[0]);
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(page);

            var links = doc.DocumentNode
                .SelectNodes("//a")
                .Select(p => p.GetAttributeValue("href", "not found"))
                .Distinct()
                .Select(x => URIHelper.GetAbsoluteLink(x, sourceUrl))
                .Where(x => x != null)
                .ToArray();

            var linksToProceed = links
                .Where(link => URIHelper.IsLinkLocalForDomain(link, sourceUrl))
                .ToArray();

            return new ParseResult(links, linksToProceed);

        }
    }
}
