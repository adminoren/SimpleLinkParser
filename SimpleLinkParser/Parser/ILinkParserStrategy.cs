namespace SimpleLinkParser.Parser
{
    public interface ILinkParserStrategy
    {
        string Name { get; }
        ParseResult GetLinks(string page, string sourceUrl);
    }
}
