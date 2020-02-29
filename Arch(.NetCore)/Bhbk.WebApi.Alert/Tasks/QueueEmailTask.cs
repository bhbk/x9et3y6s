using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure;
using Bhbk.Lib.Identity.Models.Alert;
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
                    status = typeof(QueueEmailTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), stoppingToken);

                try
                {
                    var queue = new ConcurrentQueue<tbl_QueueEmails>();

                    /*
                     * async database calls from background services should be
                     * avoided so threading issues do not occur.
                     * 
                     * when calling scoped service (unit of work) from a singleton
                     * service (background task) wrap in using block to mimic transient.
                     */

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        foreach (var entry in uow.QueueEmails.Get(new QueryExpression<tbl_QueueEmails>()
                            .Where(x => x.Created < DateTime.Now.AddSeconds(-(_expire))).ToLambda()))
                        {
                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                                "The email was created on " + entry.Created + " and is being deleted now.");

                            uow.QueueEmails.Delete(entry);
                        }

                        uow.Commit();

                        foreach (var entry in uow.QueueEmails.Get(new QueryExpression<tbl_QueueEmails>()
                            .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                                queue.Enqueue(entry);
                    }

                    Status = JsonConvert.SerializeObject(
                        new
                        {
                            status = typeof(QueueEmailTask).Name + " contains " + queue.Count() + " email messages queued for hand-off.",
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
                                            Log.Information(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                                        }
                                        else
                                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                                "Error=" + response.StatusCode);
#elif !RELEASE
                                        uow.QueueEmails.Delete(msg);
                                        Log.Information(typeof(QueueEmailTask).Name + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                    }
                                    break;

                                case InstanceContext.End2EndTest:
                                case InstanceContext.IntegrationTest:
                                    {
                                        uow.QueueEmails.Delete(msg);
                                        Log.Information(typeof(QueueEmailTask).Name + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
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
