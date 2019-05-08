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
        public static IWebHost CreateWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .UseKestrel(options =>
                {
                    options.ConfigureEndpoints();
                })
                .UseStartup<Startup>()
                .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .CreateLogger();

            CreateWebHost(args).Run();
        }
    }
}
