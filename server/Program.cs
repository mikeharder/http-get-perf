using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Text;

namespace Server
{
    public class Program
    {
        private static readonly byte[] _payload = Encoding.UTF8.GetBytes("Hello, World!");

        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000);
                })
                .Configure(app => app.Run(context =>
                {
                    var payloadLength = _payload.Length;
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength = payloadLength;
                    return context.Response.Body.WriteAsync(_payload, 0, payloadLength);
                }))
                .Build()
                .Run();
        }
    }
}
