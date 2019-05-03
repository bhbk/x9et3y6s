using Bhbk.Lib.Identity.Internal.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainUsersTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainUsersTask(IServiceScopeFactory factory)
        {
            _factory = factory;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var scope = _factory.CreateScope();
            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:MaintainUsers:PollingDelay"]);

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
                    var scope = _factory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var disabled = (await uow.UserRepo.GetAsync(x => x.LockoutEnd < DateTime.UtcNow)).ToList();
                    var disabledCount = disabled.Count();

                    if (disabled.Any())
                    {
                        foreach (var entry in disabled.ToList())
                            await uow.UserRepo.DeleteAsync(entry.Id);

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
