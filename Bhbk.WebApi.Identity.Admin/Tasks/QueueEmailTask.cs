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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class QueueEmailTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly ConcurrentQueue<UserCreateEmail> _queue;
        private readonly SendgridProvider _provider;
        private readonly int _delay;
        private readonly bool _enabled;
        private readonly string _providerApiKey;

        public string Status { get; private set; }

        public QueueEmailTask(IIdentityContext ioc)
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

            _delay = int.Parse(_cb["Tasks:QueueEmail:PollingDelay"]);
            _enabled = bool.Parse(_cb["Tasks:QueueEmail:Enabled"]);
            _providerApiKey = _cb["Tasks:QueueEmail:ProviderApiKey"];
            _ioc = ioc;
            _queue = new ConcurrentQueue<UserCreateEmail>();
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

                foreach (var entry in _queue)
                {
                    try
                    {
                        UserCreateEmail model;

                        if (!_queue.TryPeek(out model))
                            break;

                        if (_ioc.ContextStatus == ContextType.Live)
                        {
                            var result = await _provider.TryEmailHandoff(_providerApiKey, model);

                            if (result.StatusCode == HttpStatusCode.OK)
                                if (!_queue.TryDequeue(out model))
                                    break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex.ToString());
                    }
                }

                var msg = typeof(QueueEmailTask).Name + " contains " + _queue.Count() + " email messages queued for delivery.";

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

                Log.Information(msg);
            }
        }

        public bool TryEnqueueEmail(UserCreateEmail model)
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
