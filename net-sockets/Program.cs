using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        private static int _completedRequests = 0;

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: app <url> <parallel> <warmup> <duration>");
                return;
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
            Console.WriteLine();

            var uri = new Uri(url);
            var requestBytes = Encoding.UTF8.GetBytes($"GET {uri.PathAndQuery} HTTP/1.1\r\nHost: {uri.Host}:{uri.Port}\r\n\r\n");

            var tasks = new Task[parallel];
            for (var i=0; i < parallel; i++)
            {
                tasks[i] = ExecuteRequests(uri.Host, uri.Port, requestBytes);
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

        private static async Task ExecuteRequests(string host, int port, byte[] requestBytes)
        {
            var buffer = new byte[200];

            using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(host, port);

            while (true)
            {
                await socket.SendAsync(requestBytes, SocketFlags.None);
                await socket.ReceiveAsync(buffer, SocketFlags.None);
                Interlocked.Increment(ref _completedRequests);
            }
        }
    }
}
