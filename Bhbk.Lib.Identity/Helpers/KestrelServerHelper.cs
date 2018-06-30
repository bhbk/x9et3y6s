using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Bhbk.Lib.Identity.Helpers
{
    public static class KestrelServerHelper
    {
        public static void ConfigureEndpoints(this KestrelServerOptions options)
        {
            var config = options.ApplicationServices.GetRequiredService<IConfiguration>();

            var endpoints = config.GetSection("HttpServer:Endpoints")
                .GetChildren()
                .ToDictionary(section => section.Key, section =>
                {
                    var endpoint = new EndPointConfig();
                    section.Bind(endpoint);

                    return endpoint;
                });

            foreach (var endpoint in endpoints)
            {
                if (endpoint.Value.Enable)
                {
                    var list = new List<IPAddress>();

                    if (endpoint.Value.Host == "localhost")
                    {
                        list.Add(IPAddress.IPv6Loopback);
                        list.Add(IPAddress.Loopback);
                    }
                    else if (IPAddress.TryParse(endpoint.Value.Host, out var address))
                        list.Add(address);
                    else
                        list.Add(IPAddress.IPv6Any);

                    foreach (var ip in list)
                    {
                        options.Listen(ip, endpoint.Value.Port,
                            listenOptions =>
                            {
                                if (endpoint.Value.Scheme == "https")
                                {
                                    listenOptions.UseHttps(CryptoHelper.CreateCertificate());
                                }
                            });
                    }
                }
            }
        }
    }

    public class EndPointConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Scheme { get; set; }
        public bool Enable { get; set; }
    }
}
