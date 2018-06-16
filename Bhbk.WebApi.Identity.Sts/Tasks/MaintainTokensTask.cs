using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainTokensTask : BackgroundService
    {
        private readonly IIdentityContext _ioc; 
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly int _interval;
        public string Status { get; private set; }

        public MaintainTokensTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _interval = int.Parse(_cb["Tasks:MaintainTokens:PollingInterval"]);
            _ioc = ioc;

            Status = string.Empty;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                try
                {
                    var invalid = _ioc.UserMgmt.Store.Context.AppUserRefresh
                        .Where(x => x.IssuedUtc > DateTime.UtcNow || x.ExpiresUtc < DateTime.UtcNow);
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        foreach (AppUserRefresh entry in invalid.ToList())
                            _ioc.UserMgmt.Store.Context.AppUserRefresh.Remove(entry);

                        _ioc.UserMgmt.Store.Context.SaveChanges();

                        Log.Information("Ran " + typeof(MaintainTokensTask).Name + " in background. Delete " + invalidCount.ToString() + " invalid refresh tokens.");
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }
        }
    }
}
