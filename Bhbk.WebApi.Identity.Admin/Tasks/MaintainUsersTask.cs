using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainUsersTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_cb["Tasks:MaintainUsers:PollingDelay"]);
            _ioc = ioc;

            var statusMsg = typeof(MaintainUsersTask).Name + " not run yet.";

            Status = JsonConvert.SerializeObject(new
            {
                status = statusMsg
            }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var disabled = _ioc.UserMgmt.Store.Context.AppUser
                        .Where(x => x.LockoutEnd < DateTime.UtcNow);
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

                        var statusMsg = typeof(MaintainUsersTask).Name + " success on " + DateTime.Now.ToString() + ". Enabled "
                                + disabledCount.ToString() + " users with expired lock-outs.";

                        Status = JsonConvert.SerializeObject(new
                        {
                            status = statusMsg
                        }, _serializer);

                        Log.Information(statusMsg);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}
