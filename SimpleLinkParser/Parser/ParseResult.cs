namespace SimpleLinkParser.Parser
{
    public class ParseResult
    {
        public string[] Links { get; private set; }
        public string[] LinksToParseNext { get; private set; }

        public ParseResult(string[] links, string[] linksToParseNext)
        {
            this.Links = links;
            this.LinksToParseNext = linksToParseNext;
        }
    }
}
