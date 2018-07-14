using Bhbk.Lib.Helpers.FileSystem;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Identity.Providers;
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
        private readonly FileInfo _api = Search.DefaultPaths("appsettings-api.json");
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext _ioc;
        private readonly JsonSerializerSettings _serializer;
        private readonly ConcurrentQueue<UserCreateEmail> _queue;
        private readonly SendgridProvider _provider;
        private readonly int _delay, _expire;
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

            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            _delay = int.Parse(_conf["Tasks:QueueEmail:PollingDelay"]);
            _expire = int.Parse(_conf["Tasks:QueueEmail:ExpireDelay"]);
            _enabled = bool.Parse(_conf["Tasks:QueueEmail:Enabled"]);
            _providerApiKey = _conf["Tasks:QueueEmail:ProviderApiKey"];
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

                        if (model.Created < DateTime.Now.AddSeconds(-(_expire)))
                        {
                            _queue.TryDequeue(out model);

                            Log.Warning(typeof(QueueEmailTask).Name + " hand-off of email (ID=" + model.Id.ToString() + ") to upstream provider failed many times. The email was created on "
                                + model.Created + " and is being deleted now.");

                            continue;
                        }

                        if (_ioc.ContextStatus == ContextType.Live)
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
