using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : CustomIdentityTask
    {
        private static IIdentityContext _ioc;

        public MaintainUsersTask(IIdentityContext ioc)
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

                    var disabledUsers = _ioc.UserMgmt.Store.Context.AppUser.Where(x => x.LockoutEnd < DateTime.UtcNow);

                    if (disabledUsers.Any())
                    {
                        Log.Information("Background task enable " + disabledUsers.Count().ToString() + " users after lockout end.");

                        foreach (AppUser entry in disabledUsers.ToList())
                        {
                            entry.LockoutEnabled = false;
                            entry.LockoutEnd = null;
                            _ioc.UserMgmt.Store.Context.Entry(entry).State = EntityState.Modified;
                        }

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
