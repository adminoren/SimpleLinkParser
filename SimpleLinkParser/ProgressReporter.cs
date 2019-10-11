using System;

namespace SimpleLinkParser
{
    public class ProgressReporter : IProgress<string>
    {
        public void Report(string value)
        {
            Console.WriteLine(value);
        }
    }
}
