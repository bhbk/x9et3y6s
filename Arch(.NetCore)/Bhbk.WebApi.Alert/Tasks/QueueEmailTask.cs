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
    public class QueueEmailTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay, _expire;
        private readonly bool _enabled;
        private readonly string _providerApiKey;

        public string Status { get; private set; }

        public QueueEmailTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            _factory = factory;
            _delay = int.Parse(conf["Tasks:QueueEmail:PollingDelay"]);
            _expire = int.Parse(conf["Tasks:QueueEmail:ExpireDelay"]);
            _enabled = bool.Parse(conf["Tasks:QueueEmail:Enabled"]);
            _providerApiKey = conf["Tasks:QueueEmail:ProviderApiKey"];
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
                    var queue = new ConcurrentQueue<tbl_QueueEmails>();

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        foreach (var entry in uow.QueueEmails.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueEmails>()
                            .Where(x => x.Created < DateTime.Now.AddSeconds(-(_expire))).ToLambda()))
                        {
                            Log.Warning(callPath + " hand-off of email (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                                "The email was created on " + entry.Created + " and is being deleted now.");

                            uow.QueueEmails.Delete(entry);
                        }

                        uow.Commit();

                        foreach (var entry in uow.QueueEmails.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueEmails>()
                            .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                                queue.Enqueue(entry);
                    }

                    Status = JsonConvert.SerializeObject(
                        new
                        {
                            status = callPath + " contains " + queue.Count() + " email messages queued for hand-off.",
                            queue = queue.Select(x => new {
                                Id = x.Id.ToString(),
                                Created = x.Created,
                                From = x.FromDisplay + " <" + x.FromEmail + ">",
                                To = x.ToDisplay + " <" + x.ToEmail + ">",
                                Subject = x.Subject
                            })
                        }, _serializer);

                    if (!_enabled || queue.IsEmpty)
                        continue;

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var provider = new SendGridProvider();

                        foreach (var msg in queue.OrderBy(x => x.Created))
                        {
                            switch (uow.InstanceType)
                            {
                                case InstanceContext.DeployedOrLocal:
                                    {
#if RELEASE
                                        var response = provider.TryEmailHandoff(_providerApiKey, msg).Result;

                                        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                                        {
                                            uow.QueueEmails.Delete(msg);
                                            Log.Information(callPath + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                                        }
                                        else
                                            Log.Warning(callPath + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                                "Error=" + response.StatusCode);
#elif !RELEASE
                                        uow.QueueEmails.Delete(msg);
                                        Log.Information(callPath + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                    }
                                    break;

                                case InstanceContext.End2EndTest:
                                case InstanceContext.IntegrationTest:
                                    {
                                        uow.QueueEmails.Delete(msg);
                                        Log.Information(callPath + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
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

        public bool TryEnqueueEmail(EmailV1 model)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var email = mapper.Map<tbl_QueueEmails>(model);

                    uow.QueueEmails.Create(email);
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
