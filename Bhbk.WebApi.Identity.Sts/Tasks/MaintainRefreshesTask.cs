using Bhbk.Lib.Identity.Data.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainRefreshesTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainRefreshesTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _delay = int.Parse(conf["Tasks:MaintainRefreshes:PollingDelay"]);
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainRefreshesTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), stoppingToken);

                try
                {
                    /*
                     * async database calls from background services should be
                     * avoided so threading issues do not occur.
                     * 
                     * when calling scoped service (unit of work) from a singleton
                     * service (background task) wrap in using block to mimic transient.
                     */

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                        var invalid = (uow.RefreshRepo.GetAsync(x => x.ValidFromUtc > DateTime.UtcNow || x.ValidToUtc < DateTime.UtcNow).Result).ToList();
                        var invalidCount = invalid.Count();

                        if (invalid.Any())
                        {
                            foreach (var entry in invalid.ToList())
                                uow.RefreshRepo.DeleteAsync(entry.Id).Wait();

                            uow.CommitAsync().Wait();

                            var msg = typeof(MaintainRefreshesTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                    + invalidCount.ToString() + " invalid refresh tokens.";

                            Status = JsonConvert.SerializeObject(
                                new
                                {
                                    status = msg
                                }, _serializer);

                            Log.Information(msg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
        }
    }
}
