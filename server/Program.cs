using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
                })
                .Configure(app => app.Run(async context =>
                {
                    if (HttpMethods.IsPut(context.Request.Method))
                    {
                        var reader = context.Request.BodyReader;
                        long bytesRead = 0;
                        while (true)
                        {
                            var result = await reader.ReadAsync();
                            bytesRead += result.Buffer.Length;
                            if (result.IsCompleted)
                            {
                                break;
                            }
                            else
                            {
                                reader.AdvanceTo(result.Buffer.End);
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
