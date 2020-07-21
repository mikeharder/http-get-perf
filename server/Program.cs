using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace App
{
    public class Program
    {
        private static readonly byte[] _payload = Encoding.UTF8.GetBytes("Hello, World!");
        private static readonly int _payloadLength = _payload.Length;

        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Any, 5000);
                    options.Limits.MaxRequestBodySize = null;
                })
                .Configure(app => app.Run(async context =>
                {
                    if (HttpMethods.IsPut(context.Request.Method))
                    {
                        long bytesRead = 0;

                        var reader = context.Request.BodyReader;
                        while (true)
                        {
                            var result = await reader.ReadAsync();
                            bytesRead += result.Buffer.Length;
                            reader.AdvanceTo(result.Buffer.End);
                            if (result.IsCompleted)
                            {
                                break;
                            }
                        }

                        var payload = Encoding.UTF8.GetBytes(bytesRead.ToString());
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/plain";
                        context.Response.ContentLength = payload.Length;
                        await context.Response.Body.WriteAsync(payload, 0, payload.Length);
                    }
                    else
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/plain";
                        context.Response.ContentLength = _payloadLength;
                        await context.Response.Body.WriteAsync(_payload, 0, _payloadLength);
                    }
                }))
                .Build()
                .Run();
        }
    }
}

