using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.WebApi.Alert.Providers;
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
            var provider = new SendgridProvider();

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

                        foreach (var entry in uow.UserRepo.GetQueueEmailAsync(x => x.Created < DateTime.Now.AddSeconds(-(_expire))).Result)
                        {
                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                                "The email was created on " + entry.Created + " and is being deleted now.");

                            uow.UserRepo.DeleteQueueEmailAsync(entry.Id.ToString()).Wait();
                        }

                        uow.CommitAsync().Wait();

                        foreach (var entry in uow.UserRepo.GetQueueEmailAsync(x => x.SendAt < DateTime.Now).Result)
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
                                            uow.UserRepo.DeleteEmailAsync(msg.Id.ToString()).Wait();
                                            Log.Information(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                                        }
                                        else
                                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                                "Error=" + response.StatusCode);
#elif !RELEASE
                                        uow.UserRepo.DeleteQueueEmailAsync(msg.Id.ToString()).Wait();
                                        Log.Information(typeof(QueueEmailTask).Name + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                    }
                                    break;

                                case InstanceContext.UnitTest:
                                    {
                                        uow.UserRepo.DeleteQueueEmailAsync(msg.Id.ToString()).Wait();
                                        Log.Information(typeof(QueueEmailTask).Name + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
                                    }
                                    break;

                                default:
                                    throw new NotImplementedException();

                            }
                        }

                        uow.CommitAsync().Wait();
                    }

                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }

        public bool TryEnqueueEmail(tbl_QueueEmails model)
        {
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    uow.UserRepo.CreateQueueEmailAsync(model).Wait();
                    uow.CommitAsync().Wait();
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
