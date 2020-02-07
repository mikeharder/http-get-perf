using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static int _completedRequests = 0;

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: app <url> <parallel> <duration>");
                return;
            }

            var url = args[0];
            var parallel = args.Length >= 2 ? Int32.Parse(args[1]) : 32;
            var duration = args.Length >= 3 ? Int32.Parse(args[2]) : 10;

            Console.WriteLine($"Url: {url}");
            Console.WriteLine($"Parallel: {parallel}");
            Console.WriteLine($"Duration: {duration}");
            Console.WriteLine();

            var tasks = new Task[parallel];
            var stopwatch = Stopwatch.StartNew();

            for (var i=0; i < parallel; i++)
            {
                tasks[i] = ExecuteRequests(url);
            }
            await Task.Delay(TimeSpan.FromSeconds(duration));

            stopwatch.Stop();

            var completedRequests = _completedRequests;
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var requestsPerSecond = _completedRequests / elapsedSeconds;

            Console.WriteLine($"Completed {completedRequests:N0} requests in {elapsedSeconds:N2} seconds ({requestsPerSecond:N0} req/s)");
        }

        private static async Task ExecuteRequests(string url)
        {
            while (true)
            {
                await _httpClient.GetStringAsync(url);
                Interlocked.Increment(ref _completedRequests);
            }
        }
    }
}
