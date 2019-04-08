using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Core.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Program
    {
        private static IConfigurationRoot _conf;
        private static FileInfo _api;

        public static IWebHost CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(_conf)
                .UseStartup<Startup>()
                .UseKestrel(options =>
                {
                    options.ConfigureEndpoints();
                })
                .UseApplicationInsights()
                .CaptureStartupErrors(true)
                .PreferHostingUrls(true)
                .Build();

        public static void Main(string[] args)
        {
            _api = SearchRoots.ByAssemblyContext("appsettings.json");
            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .CreateLogger();

            CreateWebHost(args).Run();
        }
    }
}
