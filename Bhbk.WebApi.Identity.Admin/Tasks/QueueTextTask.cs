using Bhbk.Lib.Identity.Externals;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class QueueTextTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly ConcurrentQueue<UserCreateText> _queue;
        private readonly TwilioProvider _provider;
        private readonly int _delay;
        private readonly bool _enabled;
        private readonly string _providerSid, _providerToken;

        public string Status { get; private set; }

        public QueueTextTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_cb["Tasks:QueueText:PollingDelay"]);
            _enabled = bool.Parse(_cb["Tasks:QueueText:Enabled"]);
            _providerSid = _cb["Tasks:QueueText:ProviderSid"];
            _providerToken = _cb["Tasks:QueueText:ProviderToken"];
            _ioc = ioc;
            _queue = new ConcurrentQueue<UserCreateText>();
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

                foreach (var entry in _queue)
                {
                    try
                    {
                        UserCreateText model;

                        if (!_queue.TryPeek(out model))
                            break;

                        if (_ioc.ContextStatus == ContextType.Live)
                        {
                            await _provider.TryTextHandoff(_providerSid, _providerToken, model);

                            if (!_queue.TryDequeue(out model))
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                var msg = typeof(QueueTextTask).Name + " contains " + _queue.Count() + " text messages queued for delivery.";

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

                Log.Information(msg);
            }
        }

        public bool TryEnqueueText(UserCreateText model)
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

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());

                return false;
            }
        }
    }
}
