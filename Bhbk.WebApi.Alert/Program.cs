using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace Bhbk.WebApi.Alert
{
    public class Program
    {
        private static IConfigurationRoot _conf;

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(_conf)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.ConfigureEndpoints();
                })
                .UseApplicationInsights()
                .CaptureStartupErrors(true)
                .PreferHostingUrls(false)
                .Build();

        public static void Main(string[] args)
        {
            var api = SearchRoots.ByAssemblyContext("appsettings.json");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.LiterateConsole()
                .WriteTo.RollingFile(api.DirectoryName + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .CreateLogger();

            _conf = new ConfigurationBuilder()
                .SetBasePath(api.DirectoryName)
                .AddJsonFile(api.Name, optional: false, reloadOnChange: true)
                .Build();

            BuildWebHost(args).Run();
        }
    }
}
