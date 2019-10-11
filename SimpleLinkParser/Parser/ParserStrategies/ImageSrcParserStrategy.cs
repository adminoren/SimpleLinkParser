using HtmlAgilityPack;
using System.Linq;

namespace SimpleLinkParser.Parser.ParserStrategies
{
    public class ImageSrcParserStrategy : ILinkParserStrategy
    {
        public string Name => nameof(ImageSrcParserStrategy);

        ParseResult ILinkParserStrategy.GetLinks(string page, string sourceUrl)
        {
            var emptyResult = new ParseResult(new string[0], new string[0]);

            if (string.IsNullOrWhiteSpace(page))
            {
                return emptyResult;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(page);

            try
            {
                var links = doc.DocumentNode
                    .SelectNodes("//img")
                    .Where(x => x != null)
                    .Select(p => p.GetAttributeValue("src", "not found"))
                    .Distinct()
                    .Select(x => URIHelper.GetAbsoluteLink(x, sourceUrl))
                    .Where(x => x != null)
                    .ToArray();

                var linksToProceed = doc.DocumentNode
                    .SelectNodes("//a")
                    .Select(p => p.GetAttributeValue("href", "not found"))
                    .Distinct()
                    .Select(x => URIHelper.GetAbsoluteLink(x, sourceUrl))
                    .Where(link => link != null && URIHelper.IsLinkLocalForDomain(link, sourceUrl))
                    .ToArray();

                return new ParseResult(links, linksToProceed);
            }
            catch
            {
            }

            return emptyResult;
        }
    }
}
