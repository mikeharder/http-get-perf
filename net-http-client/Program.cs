using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        private static HttpClient _httpClient;
        private static int _completedRequests = 0;

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: app <url> <parallel> <warmup> <duration> [--insecure]");
                return;
            }

            var insecure = false;
            if (args.Contains("--insecure", StringComparer.OrdinalIgnoreCase))
            {
                insecure = true;
                args = args.Where(a => !a.Equals("--insecure", StringComparison.OrdinalIgnoreCase)).ToArray();
            }

            var url = args[0];
            var parallel = args.Length >= 2 ? Int32.Parse(args[1]) : 64;
            var warmup = args.Length >= 3 ? Int32.Parse(args[2]) : 10;
            var duration = args.Length >= 4 ? Int32.Parse(args[3]) : 10;

            Console.WriteLine($"=== Parameters ===");
            Console.WriteLine($"Url: {url}");
            Console.WriteLine($"Parallel: {parallel}");
            Console.WriteLine($"Warmup: {warmup}");
            Console.WriteLine($"Duration: {duration}");
            Console.WriteLine($"Insecure: {insecure}");
            Console.WriteLine($"GCSettings.IsServerGC: {GCSettings.IsServerGC}");
            Console.WriteLine();

            if (insecure)
            {
                var httpClientHandler = new HttpClientHandler();
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                _httpClient = new HttpClient(httpClientHandler);
            }
            else
            {
                _httpClient = new HttpClient();
            }

            var tasks = new Task[parallel];
            for (var i=0; i < parallel; i++)
            {
                tasks[i] = ExecuteRequests(url);
            }

            await CollectResults("Warmup", warmup);
            await CollectResults("Test", duration);
        }

        private static async Task CollectResults(string title, int duration) {
            Console.WriteLine($"=== {title} ===");
            
            var stopwatch = Stopwatch.StartNew();
            Interlocked.Exchange(ref _completedRequests, 0);
            
            await Task.Delay(TimeSpan.FromSeconds(duration));
            
            stopwatch.Stop();
            
            PrintResults(_completedRequests, stopwatch.Elapsed);
        }

        private static void PrintResults(int completedRequests, TimeSpan elapsed) {
            var elapsedSeconds = elapsed.TotalSeconds;
            var requestsPerSecond = completedRequests / elapsedSeconds;

            Console.WriteLine($"Completed {completedRequests:N0} requests in {elapsedSeconds:N2} seconds ({requestsPerSecond:N0} req/s)");
            Console.WriteLine();
        }

        private static async Task ExecuteRequests(string url)
        {
            while (true)
            {
                await ExecuteRequest(url);
                Interlocked.Increment(ref _completedRequests);
            }
        }

        private static async Task ExecuteRequest(string url)
        {
            await _httpClient.GetStringAsync(url);
        }
    }
}
