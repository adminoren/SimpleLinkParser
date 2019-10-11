using SimpleLinkParser.Parser;
using SimpleLinkParser.Parser.ParserStrategies;
using System;

namespace SimpleLinkParser
{
    static class Program
    {
        static void Main(string[] args)
        {
            var parser = new LinkParser();

            var domains = new string[]
            {
                "https://mail.ru/"
            };

            var strategies = new ILinkParserStrategy[]
            {
                new BaseParserStrategy(),
                new ExcludeValueParserStrategie("profile"),
                new ImageSrcParserStrategy(),
                new NotLessThenPageSizeParserStrategy(1024 * 200)
            };

            Console.CancelKeyPress += new ConsoleCancelEventHandler((object sender, ConsoleCancelEventArgs cancelArgs) =>
            {
                parser.Stop();
                cancelArgs.Cancel = true;
            });

            parser.Parse(domains, "result.txt", strategies, new ProgressReporter());

            Console.WriteLine("Parsing is over");
            Console.ReadLine();
        }

    }
}
