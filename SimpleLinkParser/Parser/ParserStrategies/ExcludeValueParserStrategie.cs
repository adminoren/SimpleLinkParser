using HtmlAgilityPack;
using System;
using System.Linq;

namespace SimpleLinkParser.Parser.ParserStrategies
{
    class ExcludeValueParserStrategie : ILinkParserStrategy
    {
        private readonly string _excludedValue;


        public ExcludeValueParserStrategie(string excludedValue)
        {
            _excludedValue = excludedValue;
        }

        public string Name => $"{nameof(ExcludeValueParserStrategie)}: {_excludedValue}";

        public ParseResult GetLinks(string page, string sourceUrl)
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
                .Where(x => x != null && !x.Contains(_excludedValue, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            var linksToProceed = links
                .Where(link => URIHelper.IsLinkLocalForDomain(link, sourceUrl))
                .ToArray();

            return new ParseResult(links, linksToProceed);
        }
    }
}
