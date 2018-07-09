using Bhbk.Lib.Identity.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace Bhbk.WebApi.Identity.Admin
{
    public class Program
    {
        private static FileInfo _api = FileSystemHelper.SearchPaths("appsettings-api.json");
        private static IConfigurationRoot _conf;

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options => 
                {
                    options.ConfigureEndpoints();
                })
                .UseConfiguration(_conf)
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .CaptureStartupErrors(true)
                .PreferHostingUrls(false)
                .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(_api.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .CreateLogger();

            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            BuildWebHost(args).Run();
        }
    }
}
