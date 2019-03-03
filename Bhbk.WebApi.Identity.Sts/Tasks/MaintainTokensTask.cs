using Bhbk.Lib.Identity.Internal.EntityModels;
using Bhbk.Lib.Identity.Internal.Interfaces;
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
    public class MaintainTokensTask : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }


        public MaintainTokensTask(IServiceCollection sc, IConfigurationRoot conf)
        {
            if (sc == null)
                throw new ArgumentNullException();

            _sp = sc.BuildServiceProvider();
            _delay = int.Parse(conf["Tasks:MaintainTokens:PollingDelay"]);

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainTokensTask).Name + " not run yet."
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

                    var invalid = (await uow.UserRepo.GetRefreshTokensAsync(x => x.IssuedUtc > DateTime.UtcNow || x.ExpiresUtc < DateTime.UtcNow)).ToList();
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        foreach (var entry in invalid.ToList())
                            await uow.UserRepo.RemoveRefreshTokenAsync(entry.Id);

                        await uow.CommitAsync();

                        var msg = typeof(MaintainTokensTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
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
