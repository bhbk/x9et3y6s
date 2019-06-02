using Bhbk.Lib.Hosting.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Diagnostics;
using System.IO;

namespace Bhbk.WebApi.Identity.Me
{
    public class Program
    {
        public static IWebHostBuilder CreateIISHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .UseIISIntegration()
            .UseStartup<Startup>();

        public static IWebHostBuilder CreateKestrelHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .UseKestrel(options =>
            {
                options.ConfigureEndpoints();
            })
            .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "appdebug.log", retainedFileCountLimit: 7)
                .CreateLogger();

            var process = Process.GetCurrentProcess();

            if (process.ProcessName.ToLower().Contains("iis"))
                CreateIISHostBuilder(args).Build().Run();

            else
                CreateKestrelHostBuilder(args).Build().Run();
        }
    }
}
