using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;
using Microsoft.Extensions.Hosting;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private readonly int _interval;
        public string Status { get; private set; }

        public MaintainUsersTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new NullReferenceException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _interval = int.Parse(_cb["Tasks:MaintainUsers:PollingInterval"]);
            _ioc = ioc;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                DoWork();
            }
        }

        public void DoWork()
        {
            try
            {
                var disabled = _ioc.UserMgmt.Store.Context.AppUser.Where(x => x.LockoutEnd < DateTime.UtcNow);
                var disabledCount = disabled.Count();

                if (disabled.Any())
                {
                    foreach (AppUser entry in disabled.ToList())
                    {
                        entry.LockoutEnabled = false;
                        entry.LockoutEnd = null;
                        _ioc.UserMgmt.Store.Context.Entry(entry).State = EntityState.Modified;
                    }

                    _ioc.UserMgmt.Store.Context.SaveChanges();

                    Log.Information("Task " + typeof(MaintainUsersTask).Name + ". Enabled " + disabledCount.ToString() + " users with lockout-end expire.");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, BaseLib.Statics.MsgSystemExceptionCaught);
            }
        }
    }
}
