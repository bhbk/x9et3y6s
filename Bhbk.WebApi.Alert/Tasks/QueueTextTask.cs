using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
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
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Tasks
{
    public class QueueTextTask : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly JsonSerializerSettings _serializer;
        private readonly ConcurrentQueue<TextCreate> _queue;
        private readonly TwilioProvider _provider;
        private readonly int _delay, _expire;
        private readonly bool _enabled;
        private readonly string _providerSid, _providerToken;

        public string Status { get; private set; }

        public QueueTextTask(IServiceCollection sc)
        {
            if (sc == null)
                throw new ArgumentNullException();

            _sp = sc.BuildServiceProvider();
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var conf = _sp.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:QueueText:PollingDelay"]);
            _expire = int.Parse(conf["Tasks:QueueText:ExpireDelay"]);
            _enabled = bool.Parse(conf["Tasks:QueueText:Enabled"]);
            _providerSid = conf["Tasks:QueueText:ProviderSid"];
            _providerToken = conf["Tasks:QueueText:ProviderToken"];
            _queue = new ConcurrentQueue<TextCreate>();
            _provider = new TwilioProvider();

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

                var uow = _sp.GetRequiredService<IIdentityUnitOfWork>();

                foreach (var entry in _queue)
                {
                    try
                    {
                        TextCreate model;

                        if (!_queue.TryPeek(out model))
                            break;

                        if (model.Created < DateTime.Now.AddSeconds(-(_expire)))
                        {
                            _queue.TryDequeue(out model);

                            Log.Warning(typeof(QueueTextTask).Name + " hand-off of text (ID=" + model.Id.ToString() + ") to upstream provider failed many times. The text was created on "
                                + model.Created + " and is being deleted now.");

                            continue;
                        }

                        if (uow.InstanceType == InstanceContext.DeployedOrLocal)
                        {
                            await _provider.TryTextHandoff(_providerSid, _providerToken, model);

                            if (!_queue.TryDequeue(out model))
                                break;

                            Log.Information(typeof(QueueTextTask).Name + " hand-off of text (ID=" + model.Id.ToString() + ") to upstream provider was successfull.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                var msg = typeof(QueueTextTask).Name + " contains " + _queue.Count() + " text messages queued for hand-off.";

                Status = JsonConvert.SerializeObject(
                    new
                    {
                        status = msg,
                        queue = _queue.Select(x => new {
                            Id = x.Id.ToString(),
                            Created = x.Created,
                            From = x.FromPhoneNumber,
                            To = x.ToPhoneNumber
                        })
                    }, _serializer);
            }
        }

        public bool TryEnqueueText(TextCreate model)
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
