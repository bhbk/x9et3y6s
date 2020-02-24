using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Data.EFCore.Services;
using Bhbk.WebApi.Alert.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Tasks
{
    public class QueueTextTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay, _expire;
        private readonly bool _enabled;
        private readonly string _providerSid, _providerToken;

        public string Status { get; private set; }

        public QueueTextTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _delay = int.Parse(conf["Tasks:QueueText:PollingDelay"]);
            _expire = int.Parse(conf["Tasks:QueueText:ExpireDelay"]);
            _enabled = bool.Parse(conf["Tasks:QueueText:Enabled"]);
            _providerSid = conf["Tasks:QueueText:ProviderSid"];
            _providerToken = conf["Tasks:QueueText:ProviderToken"];
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(QueueTextTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), stoppingToken);

                try
                {
                    var queue = new ConcurrentQueue<tbl_QueueTexts>();

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

                        foreach (var entry in uow.QueueTexts.Get(new QueryExpression<tbl_QueueTexts>()
                            .Where(x => x.Created < DateTime.Now.AddSeconds(-(_expire))).ToLambda()))
                        {
                            Log.Warning(typeof(QueueTextTask).Name + " hand-off of text (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                                "The text was created on " + entry.Created + " and is being deleted now.");

                            uow.QueueTexts.Delete(entry);
                        }

                        uow.Commit();

                        foreach (var entry in uow.QueueTexts.Get(new QueryExpression<tbl_QueueTexts>()
                            .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                                queue.Enqueue(entry);
                    }

                    Status = JsonConvert.SerializeObject(
                        new
                        {
                            status = typeof(QueueTextTask).Name + " contains " + queue.Count() + " text messages queued for hand-off.",
                            queue = queue.Select(x => new {
                                Id = x.Id.ToString(),
                                Created = x.Created,
                                From = x.FromPhoneNumber,
                                To = x.ToPhoneNumber
                            })
                        }, _serializer);

                    if (!_enabled || queue.IsEmpty)
                        continue;

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();
                        var provider = new TwilioProvider();

                        foreach (var msg in queue.OrderBy(x => x.Created))
                        {
                            switch (uow.InstanceType)
                            {
                                case InstanceContext.DeployedOrLocal:
                                    {
#if RELEASE
                                        await provider.TryTextHandoff(_providerSid, _providerToken, msg);

                                        uow.QueueTexts.Delete(msg);
                                        Log.Information(typeof(QueueTextTask).Name + " hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
#elif !RELEASE
                                        uow.QueueTexts.Delete(msg);
                                        Log.Information(typeof(QueueTextTask).Name + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                    }
                                    break;

                                case InstanceContext.UnitTest:
                                    {
                                        uow.QueueTexts.Delete(msg);
                                        Log.Information(typeof(QueueTextTask).Name + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException();
                            }
                        }

                        uow.Commit();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        public bool TryEnqueueText(tbl_QueueTexts model)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUoWService>();

                    uow.QueueTexts.Create(model);
                    uow.Commit();
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            return false;
        }
    }
}
