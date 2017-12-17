using Bhbk.Lib.Identity.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Program
    {
        private static IConfigurationRoot Config;

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.ConfigureEndpoints();
                })
                .UseConfiguration(Config)
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .PreferHostingUrls(false)
                .Build();

        public static void Main(string[] args)
        {
            var location = FileSystemHelper.SearchUsualPaths("appsettings.json");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(location.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log",
                    fileSizeLimitBytes: 1048576, retainedFileCountLimit: 7)
                .CreateLogger();

            Config = new ConfigurationBuilder()
                .SetBasePath(location.DirectoryName)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            BuildWebHost(args).Run();
        }
    }
}
