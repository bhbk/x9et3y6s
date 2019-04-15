using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
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
    public class MaintainStatesTask : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }


        public MaintainStatesTask(IServiceCollection sc, IConfigurationRoot conf)
        {
            if (sc == null)
                throw new ArgumentNullException();

            _sp = sc.BuildServiceProvider();
            _delay = int.Parse(conf["Tasks:MaintainStates:PollingDelay"]);

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainStatesTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var uow = (IIdentityUnitOfWork<IdentityDbContext>)_sp.GetRequiredService<IIdentityUnitOfWork<IdentityDbContext>>();

                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                    var invalid = (await uow.StateRepo.GetAsync(x => x.ValidFromUtc > DateTime.UtcNow || x.ValidToUtc < DateTime.UtcNow)).ToList();
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        foreach (var entry in invalid.ToList())
                            await uow.StateRepo.DeleteAsync(entry.Id);

                        await uow.CommitAsync();

                        var msg = typeof(MaintainStatesTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + invalidCount.ToString() + " invalid states.";

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
