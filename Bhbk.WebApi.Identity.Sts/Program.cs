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
        private static IConfigurationRoot _cb;
        private static FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.ConfigureEndpoints();
                })
                .UseConfiguration(_cb)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .CaptureStartupErrors(true)
                .PreferHostingUrls(false)
                .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(_cf.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log",
                    fileSizeLimitBytes: 1048576, retainedFileCountLimit: 7)
                .CreateLogger();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            BuildWebHost(args).Run();
        }
    }
}
