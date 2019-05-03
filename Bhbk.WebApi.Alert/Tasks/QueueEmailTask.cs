using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Models.Alert;
using Bhbk.WebApi.Alert.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Tasks
{
    public class QueueEmailTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly ConcurrentQueue<EmailCreate> _queue;
        private readonly SendgridProvider _provider;
        private readonly int _delay, _expire;
        private readonly bool _enabled;
        private readonly string _providerApiKey;

        public string Status { get; private set; }

        public QueueEmailTask(IServiceScopeFactory factory)
        {
            _factory = factory;
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var scope = _factory.CreateScope();
            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:QueueEmail:PollingDelay"]);
            _expire = int.Parse(conf["Tasks:QueueEmail:ExpireDelay"]);
            _enabled = bool.Parse(conf["Tasks:QueueEmail:Enabled"]);
            _providerApiKey = conf["Tasks:QueueEmail:ProviderApiKey"];
            _queue = new ConcurrentQueue<EmailCreate>();
            _provider = new SendgridProvider();

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(QueueEmailTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                if (!_enabled || _queue.IsEmpty)
                    continue;

                var scope = _factory.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                foreach (var entry in _queue)
                {
                    try
                    {
                        EmailCreate model;

                        if (!_queue.TryPeek(out model))
                            break;

                        if (model.Created < DateTime.Now.AddSeconds(-(_expire)))
                        {
                            _queue.TryDequeue(out model);

                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + model.Id.ToString() + ") to upstream provider failed many times. The email was created on "
                                + model.Created + " and is being deleted now.");

                            continue;
                        }

                        if (uow.InstanceType == InstanceContext.DeployedOrLocal)
                        {
                            var result = await _provider.TryEmailHandoff(_providerApiKey, model);

                            if (result.StatusCode == HttpStatusCode.Accepted)
                            {
                                if (!_queue.TryDequeue(out model))
                                    break;

                                Log.Information(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + model.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            else
                                Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + model.Id.ToString() + ") to upstream provider failed. Error=" + result.StatusCode);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                var msg = typeof(QueueEmailTask).Name + " contains " + _queue.Count() + " email messages queued for hand-off.";

                Status = JsonConvert.SerializeObject(
                    new
                    {
                        status = msg,
                        queue = _queue.Select(x => new {
                            Id = x.Id.ToString(),
                            Created = x.Created,
                            From = x.FromDisplay + " <" + x.FromEmail + ">",
                            To = x.ToDisplay + " <" + x.ToEmail + ">",
                            Subject = x.Subject
                        })
                    }, _serializer);
            }
        }

        public bool TryEnqueueEmail(EmailCreate model)
        {
            try
            {
                //set unique id for message...
                model.Id = Guid.NewGuid();
                model.Created = DateTime.Now;

                _queue.Enqueue(model);

                //verify message is in queue...
                if (_queue.Where(x => x.Id == model.Id).Any())
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
