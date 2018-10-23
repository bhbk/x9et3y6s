﻿using Bhbk.Lib.Core.FileSystem;
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
        private static FileInfo _api = SearchRoots.ByAssemblyContext("appsettings-api.json");
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
                .PreferHostingUrls(true)
                .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
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
