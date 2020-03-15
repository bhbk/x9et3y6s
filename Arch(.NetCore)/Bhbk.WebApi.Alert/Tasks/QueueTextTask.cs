using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
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
using System.Reflection;
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
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

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
                    status = callPath + " not run yet."
                }, _serializer);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            while (!cancellationToken.IsCancellationRequested)
            {
#if DEBUG
                Log.Information($"'{callPath}' sleeping for {TimeSpan.FromSeconds(_delay)}");
#endif
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);
#if DEBUG
                Log.Information($"'{callPath}' running");
#endif
                try
                {
                    var queue = new ConcurrentQueue<tbl_QueueTexts>();

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        foreach (var entry in uow.QueueTexts.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueTexts>()
                            .Where(x => x.Created < DateTime.Now.AddSeconds(-(_expire))).ToLambda()))
                        {
                            Log.Warning(typeof(QueueTextTask).Name + " hand-off of text (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                                "The text was created on " + entry.Created + " and is being deleted now.");

                            uow.QueueTexts.Delete(entry);
                        }

                        uow.Commit();

                        foreach (var entry in uow.QueueTexts.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueTexts>()
                            .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                                queue.Enqueue(entry);
                    }

                    Status = JsonConvert.SerializeObject(
                        new
                        {
                            status = callPath + " contains " + queue.Count() + " text messages queued for hand-off.",
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
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
                                        Log.Information(callPath + " hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
#elif !RELEASE
                                        uow.QueueTexts.Delete(msg);
                                        Log.Information(callPath + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                    }
                                    break;

                                case InstanceContext.End2EndTest:
                                case InstanceContext.IntegrationTest:
                                    {
                                        uow.QueueTexts.Delete(msg);
                                        Log.Information(callPath + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
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

                /*
                 * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
                 */
                GC.Collect();
            }
        }

        public bool TryEnqueueText(TextV1 model)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var text = mapper.Map<tbl_QueueTexts>(model);

                    uow.QueueTexts.Create(text);
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
