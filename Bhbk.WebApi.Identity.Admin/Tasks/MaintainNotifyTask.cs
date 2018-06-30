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
    public class MaintainNotifyTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly ConcurrentQueue<UserCreateEmail> _queueEmail;
        private readonly ConcurrentQueue<UserCreateText> _queueText;
        private readonly SendgridProvider _svcEmail;
        private readonly TwilioProvider _svcText;
        private readonly int _delay;
        private readonly bool _emailEnabled, _textEnabled;
        private readonly string _emailProviderApiKey, _textProviderSid, _textProviderToken;

        public string Status { get; private set; }

        public MaintainNotifyTask(IIdentityContext ioc)
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

            _delay = int.Parse(_cb["Tasks:MaintainNotify:PollingDelay"]);
            _emailEnabled = bool.Parse(_cb["Tasks:MaintainNotify:EmailEnabled"]);
            _emailProviderApiKey = _cb["Tasks:MaintainNotify:EmailProviderApiKey"];
            _textEnabled = bool.Parse(_cb["Tasks:MaintainNotify:TextEnabled"]);
            _textProviderSid = _cb["Tasks:MaintainNotify:TextProviderSid"];
            _textProviderToken = _cb["Tasks:MaintainNotify:TextProviderToken"];
            _ioc = ioc;
            _queueEmail = new ConcurrentQueue<UserCreateEmail>();
            _queueText = new ConcurrentQueue<UserCreateText>();
            _svcEmail = new SendgridProvider();
            _svcText = new TwilioProvider();

            var statusMsg = typeof(MaintainNotifyTask).Name + " not run yet.";

            Status = JsonConvert.SerializeObject(new
            {
                status = statusMsg
            }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                if (_emailEnabled && !_queueEmail.IsEmpty)
                {
                    foreach (var entry in _queueEmail)
                    {
                        try
                        {
                            UserCreateEmail email;

                            if (!_queueEmail.TryPeek(out email))
                                break;

                            if (_ioc.ContextStatus != ContextType.UnitTest)
                                await _svcEmail.TryEmailHandoff(_emailProviderApiKey, email);

                            if (!_queueEmail.TryDequeue(out email))
                                break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }

                if (_textEnabled && !_queueText.IsEmpty)
                {
                    foreach (var entry in _queueText)
                    {
                        try
                        {
                            UserCreateText text;

                            if (!_queueText.TryPeek(out text))
                                break;

                            if (_ioc.ContextStatus != ContextType.UnitTest)
                                await _svcText.TryTextHandoff(_textProviderSid, _textProviderToken, text);

                            if (!_queueText.TryDequeue(out text))
                                break;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.ToString());
                        }
                    }
                }
            }
        }

        public bool TryEnqueueEmail(UserCreateEmail model)
        {
            try
            {
                //set unique id for message...
                model.Id = Guid.NewGuid();

                _queueEmail.Enqueue(model);

                //verify message is in queue...
                if (_queueEmail.Where(x => x.Id == model.Id).Any())
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());

                return false;
            }
        }

        public bool TryEnqueueText(UserCreateText model)
        {
            try
            {
                //set unique id for message...
                model.Id = Guid.NewGuid();

                _queueText.Enqueue(model);

                //verify message is in queue...
                if (_queueText.Where(x => x.Id == model.Id).Any())
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
