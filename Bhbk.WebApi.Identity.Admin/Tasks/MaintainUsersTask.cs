using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainUsersTask(IServiceCollection sc, IConfigurationRoot conf)
        {
            if (sc == null)
                throw new ArgumentNullException();

            _sp = sc.BuildServiceProvider();
            _delay = int.Parse(conf["Tasks:MaintainUsers:PollingDelay"]);

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainUsersTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var uow = (IIdentityContext<AppDbContext>)_sp.GetRequiredService<IIdentityContext<AppDbContext>>();

                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var disabled = uow.UserMgr.Store.Context.AppUser
                        .Where(x => x.LockoutEnd < DateTime.UtcNow);

                    var disabledCount = disabled.Count();

                    if (disabled.Any())
                    {
                        foreach (AppUser entry in disabled.ToList())
                        {
                            entry.LockoutEnabled = false;
                            entry.LockoutEnd = null;
                            uow.UserMgr.Store.Context.Entry(entry).State = EntityState.Modified;
                        }

                        await uow.CommitAsync();

                        var msg = typeof(MaintainUsersTask).Name + " success on " + DateTime.Now.ToString() + ". Enabled "
                                + disabledCount.ToString() + " users with expired lock-outs.";

                        Status = JsonConvert.SerializeObject(
                            new
                            {
                                status = msg
                            }, _serializer);

                        Log.Information(msg);
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
