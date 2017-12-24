﻿using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : CustomIdentityTask
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private readonly int _interval;

        public MaintainUsersTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _ioc = ioc;
            _interval = int.Parse(_cb["Tasks:MaintainUsers:PollingInterval"]);
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                    var disabledUsers = _ioc.UserMgmt.Store.Context.AppUser.Where(x => x.LockoutEnd < DateTime.UtcNow);

                    if (disabledUsers.Any())
                    {
                        Log.Information("Task " + typeof(MaintainUsersTask).Name + ". Enabled " + disabledUsers.Count().ToString() + " users with lockout-end expire.");

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
                    Log.Error(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }
        }

        public string Status { get; private set; }
    }
}
