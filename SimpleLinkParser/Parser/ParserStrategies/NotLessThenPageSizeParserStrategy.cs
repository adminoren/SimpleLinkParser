using HtmlAgilityPack;
using System.Linq;

namespace SimpleLinkParser.Parser.ParserStrategies
{
    public class NotLessThenPageSizeParserStrategy : ILinkParserStrategy
    {
        private readonly int _pageSize;


        public NotLessThenPageSizeParserStrategy(int pageSize)
        {
            _pageSize = pageSize;
        }

        public string Name => $"{nameof(NotLessThenPageSizeParserStrategy)}: {_pageSize}";

        public ParseResult GetLinks(string page, string sourceUrl)
        {
            var emptyResult = new ParseResult(new string[0], new string[0]);

            if (string.IsNullOrWhiteSpace(page))
            {
                return emptyResult;
            }

            if (page.Length < _pageSize)
            {
                return emptyResult;
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
