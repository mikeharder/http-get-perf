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

            var uri = new Uri(url);
            var requestBytes = Encoding.UTF8.GetBytes($"GET {uri.PathAndQuery} HTTP/1.1\r\nHost: {uri.Host}:{uri.Port}\r\n\r\n");

            var tasks = new Task[parallel];
            var stopwatch = Stopwatch.StartNew();

            for (var i=0; i < parallel; i++)
            {
                tasks[i] = ExecuteRequests(uri.Host, uri.Port, requestBytes);
            }
            await Task.Delay(TimeSpan.FromSeconds(duration));

            stopwatch.Stop();

            var completedRequests = _completedRequests;
            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            var requestsPerSecond = _completedRequests / elapsedSeconds;

            Console.WriteLine($"Completed {completedRequests:N0} requests in {elapsedSeconds:N2} seconds ({requestsPerSecond:N0} req/s)");
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
