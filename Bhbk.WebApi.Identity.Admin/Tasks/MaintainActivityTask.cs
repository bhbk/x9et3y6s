﻿using Bhbk.Lib.Identity.Internal.Infrastructure;
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
    public class MaintainActivityTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay, _transient, _auditable;
        public string Status { get; private set; }

        public MaintainActivityTask(IServiceScopeFactory factory)
        {
            _factory = factory;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var scope = _factory.CreateScope();
            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:MaintainActivity:PollingDelay"]);
            _auditable = int.Parse(conf["Tasks:MaintainActivity:HoldAuditable"]);
            _transient = int.Parse(conf["Tasks:MaintainActivity:HoldTransient"]);

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainActivityTask).Name + " not run yet."
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

                    var expired = await uow.ActivityRepo.GetAsync(x => (x.Created.AddSeconds(_transient) < DateTime.Now && x.Immutable == false)
                            || (x.Created.AddSeconds(_auditable) < DateTime.Now && x.Immutable == true));

                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        foreach (var entry in expired.ToList())
                            await uow.ActivityRepo.DeleteAsync(entry.Id);

                        await uow.CommitAsync();

                        var msg = typeof(MaintainActivityTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + expiredCount.ToString() + " expired activity entries.";

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
