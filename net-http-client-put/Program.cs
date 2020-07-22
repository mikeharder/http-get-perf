using System;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;

namespace App
{
    public class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: app <url> <size>");
                return;
            }

            var url = args[0];
            var size = args.Length >= 2 ? Int32.Parse(args[1]) : 1024;

            Console.WriteLine($"=== Parameters ===");
            Console.WriteLine($"Size: {size}");
            Console.WriteLine($"GCSettings.IsServerGC: {GCSettings.IsServerGC}");
            Console.WriteLine();

            byte[] payload = new byte[size];
            (new Random(0)).NextBytes(payload);

            var sw = new Stopwatch();
            while(true) {
                sw.Restart();
                await ExecuteRequest(url, payload);
                sw.Stop();

                var elapsedSeconds = sw.Elapsed.TotalSeconds;
                var mbps = ((size / elapsedSeconds) * 8) / (1024 * 1024);

                Console.WriteLine($"Put {size:N0} bytes in {elapsedSeconds:N2} seconds ({mbps:N2} Mbps)");
            }
        }

        private static async Task ExecuteRequest(string url, byte[] payload)
        {
            using var content = new ByteArrayContent(payload);
            using var response = await _httpClient.PutAsync(url, content);
            await response.Content.ReadAsStringAsync();
        }
    }
}
