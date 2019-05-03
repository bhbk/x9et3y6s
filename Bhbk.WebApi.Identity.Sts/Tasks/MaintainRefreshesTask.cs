using Bhbk.Lib.Identity.Internal.Infrastructure;
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

        public MaintainRefreshesTask(IServiceScopeFactory factory)
        {
            _factory = factory;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var scope = _factory.CreateScope();
            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:MaintainRefreshes:PollingDelay"]);

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainRefreshesTask).Name + " not run yet."
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

                    var invalid = (await uow.RefreshRepo.GetAsync(x => x.ValidFromUtc > DateTime.UtcNow || x.ValidToUtc < DateTime.UtcNow)).ToList();
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        foreach (var entry in invalid.ToList())
                            await uow.RefreshRepo.DeleteAsync(entry.Id);

                        await uow.CommitAsync();

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
                catch (Exception ex)
                {
                    Log.Debug(ex.ToString());
                }
            }
        }
    }
}
