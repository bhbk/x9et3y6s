using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainTokensTask : CustomIdentityTask
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private readonly int _interval;

        public MaintainTokensTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _ioc = ioc;
            _interval = int.Parse(_cb["Tasks:MaintainTokens:PollingInterval"]);
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                    var expiredTokens = _ioc.UserMgmt.Store.Context.AppUserRefresh
                        .Where(x => x.IssuedUtc > DateTime.UtcNow || x.ExpiresUtc < DateTime.UtcNow);

                    if (expiredTokens.Any())
                    {
                        Log.Information("Task " + typeof(MaintainTokensTask).Name + ". Delete " +  expiredTokens.Count().ToString() + " expired refresh tokens.");

                        foreach (AppUserRefresh entry in expiredTokens.ToList())
                            _ioc.UserMgmt.Store.Context.AppUserRefresh.Remove(entry);

                        _ioc.UserMgmt.Store.Context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }
        }

        public string Status { get; private set; }
    }
}
