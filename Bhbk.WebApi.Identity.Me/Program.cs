using Bhbk.Lib.Identity.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Net;

namespace Bhbk.WebApi.Identity.Me
{
    public class Program
    {
        private static IConfigurationRoot Config;

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(service =>
                {
                    service.Listen(IPAddress.Loopback, int.Parse(Config["HttpPort"]));
                    //service.Listen(IPAddress.Loopback, int.Parse(Config["HttpsPort"]), secure =>
                    //{
                    //    secure.UseHttps("certificate.pfx", "password");
                    //});
                })
                .UseConfiguration(Config)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .CaptureStartupErrors(true)
                .Build();

        public static void Main(string[] args)
        {
            Config = new ConfigurationBuilder()
                .SetBasePath(FileHelper.SearchPaths("appsettings.json").DirectoryName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            BuildWebHost(args).Run();
        }
    }
}
