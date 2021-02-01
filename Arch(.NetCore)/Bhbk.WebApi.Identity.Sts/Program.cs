﻿using Bhbk.Lib.Hosting.Options;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.IO;

namespace Bhbk.WebApi.Identity.Sts
{
    public class Program
    {
        private static IConfiguration _conf;

        public static IWebHostBuilder CreateIISHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .ConfigureLogging((hostContext, builder) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_conf)
                    .Enrich.FromLogContext()
                    .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    .WriteTo.File($"{hostContext.HostingEnvironment.ContentRootPath}{Path.DirectorySeparatorChar}appdebug-.log",
                        retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                        fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            })
            .UseSerilog()
            .UseIISIntegration()
            .UseStartup<Startup>();

        public static IWebHostBuilder CreateKestrelHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .CaptureStartupErrors(true)
            .ConfigureLogging((hostContext, builder) =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(_conf)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.File($"{hostContext.HostingEnvironment.ContentRootPath}{Path.DirectorySeparatorChar}appdebug-.log",
                        retainedFileCountLimit: int.Parse(_conf["Serilog:RollingFile:RetainedFileCountLimit"]),
                        fileSizeLimitBytes: int.Parse(_conf["Serilog:RollingFile:FileSizeLimitBytes"]),
                        rollingInterval: RollingInterval.Day)
                    .CreateLogger();
            })
            .UseSerilog()
            .UseKestrel(options =>
            {
                options.ConfigureEndpoints();
            })
            .UseUrls()
            .UseStartup<Startup>();

        public static void Main(string[] args)
        {
            _conf = (IConfiguration)new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var process = Process.GetCurrentProcess();

            if (process.ProcessName.ToLower().Contains("iis")
                || process.ProcessName.ToLower().Contains("w3wp"))
                CreateIISHostBuilder(args).Build().Run();

            else
                CreateKestrelHostBuilder(args).Build().Run();
        }
    }
}
