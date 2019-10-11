using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleLinkParser.Parser
{

    public class LinkParser : IDisposable
    {
        private readonly HttpClient _httpClient;
        private CancellationTokenSource _cts;

        private readonly Dictionary<string, string> _linklist;
        private readonly ConcurrentDictionary<string, string> _processedLinks;
        private readonly BlockingCollection<IEnumerable<string>> _linksResultBlockingCollection;
        private readonly BlockingCollection<string> _urlsToParseBlockingCollection;


        public LinkParser()
        {
            _httpClient = new HttpClient();
            _linklist = new Dictionary<string, string>();
            _processedLinks = new ConcurrentDictionary<string, string>();
            _linksResultBlockingCollection = new BlockingCollection<IEnumerable<string>>(100);
            _urlsToParseBlockingCollection = new BlockingCollection<string>();
        }

        public void Parse(IEnumerable<string> domains, string outputFileName, ILinkParserStrategy[] strategies, IProgress<string> progress)
        {
            _cts = new CancellationTokenSource();

            foreach (var url in domains)
            {
                _urlsToParseBlockingCollection.Add(url);
            }

            //consumers of links to parse
            for (int i = 0; i <= 5; i++)
            {
                Task.Factory.StartNew(async () =>
                {
                    foreach (var url in _urlsToParseBlockingCollection.GetConsumingEnumerable())
                    {
                        var parseStrategy = GetRandomParserStrategy(strategies);

                        try
                        {
                            await ParseAsync(url, parseStrategy, _cts.Token, progress);
                        }
                        catch (OperationCanceledException)
                        {
                        }
                        catch (Exception e)
                        {
                            ReportProgress(progress, $"ERROR: {url}, {e.Message}");
                        }
                    }
                });
            }

            //consumer of result links
            while (true)
            {
                if (_linksResultBlockingCollection.TryTake(out var item, TimeSpan.FromSeconds(20)))
                {

                    var sb = new StringBuilder();

                    foreach (var newLink in item)
                    {
                        if (_linklist.TryAdd(newLink, ""))
                        {
                            sb.AppendLine(newLink);
                        }
                    }

                    using (StreamWriter sw = File.AppendText(outputFileName))
                    {
                        sw.Write(sb.ToString());
                    }
                }
                else
                {
                    Stop();
                    break;
                }
            }
        }

        public void Stop()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _urlsToParseBlockingCollection.CompleteAdding();
            _linksResultBlockingCollection.CompleteAdding();
        }


        private async Task ParseAsync(string url, ILinkParserStrategy parserStrategy, CancellationToken cancellationToken, IProgress<string> progress)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();

            using (var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false))
            {
                if (response.IsSuccessStatusCode)
                {
                    var page = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    cancellationToken.ThrowIfCancellationRequested();

                    var parseResult = parserStrategy.GetLinks(page, url);

                    _linksResultBlockingCollection.Add(parseResult.Links);

                    ReportProgress(progress, $"Got {parseResult.Links.Length} links, strategy {parserStrategy.Name} from {url}");

                    foreach (var link in parseResult.LinksToParseNext)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (_processedLinks.TryAdd(link, ""))
                        {
                            _urlsToParseBlockingCollection.Add(link);
                        }
                    }
                }
                else
                {
                    ReportProgress(progress, $"Error status code {response.StatusCode}, {url}");
                }
            }
        }

        private void ReportProgress(IProgress<string> progress, string message)
        {
            if (progress != null)
            {
                progress.Report(message);
            }
        }

        private ILinkParserStrategy GetRandomParserStrategy(ILinkParserStrategy[] strategies)
        {
            var random = new Random();
            var index = random.Next(0, strategies.Length);
            return strategies[index];
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _linksResultBlockingCollection.Dispose();
                    _urlsToParseBlockingCollection.Dispose();
                    _httpClient.Dispose();
                    if (_cts != null)
                    {
                        _cts.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~LinkParser()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
