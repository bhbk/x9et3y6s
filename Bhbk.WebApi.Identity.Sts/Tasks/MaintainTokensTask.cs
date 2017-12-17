using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainTokensTask : CustomIdentityTask
    {
        private static IIdentityContext _ioc;

        public MaintainTokensTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);

                    var expiredTokens = _ioc.UserMgmt.Store.Context.AppUserRefresh
                        .Where(x => x.IssuedUtc > DateTime.UtcNow || x.ExpiresUtc < DateTime.UtcNow);

                    if (expiredTokens.Any())
                    {
                        Log.Information("Background task delete " + expiredTokens.Count().ToString() + " stale refresh tokens.");

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
    }
}
