using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tasks
{
    public class MaintainQuotesTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly JsonSerializerSettings _serializer;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly FileInfo _qf = FileSystemHelper.SearchPaths("appquotes.json");
        private readonly HttpClient _client = new HttpClient();
        private readonly string _url = string.Empty, _output = string.Empty;
        private readonly int _delay;
        public UserQuoteOfDay QuoteOfDay { get; private set; }
        public string Status { get; private set; }

        public MaintainQuotesTask(IIdentityContext ioc)
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

            _output = _qf.DirectoryName + Path.DirectorySeparatorChar + _qf.Name;
            _delay = int.Parse(_cb["Tasks:MaintainQuotes:PollingDelay"]);
            _url = _cb["Tasks:MaintainQuotes:QuoteOfDayUrl"];
            _ioc = ioc;

            var message = typeof(MaintainQuotesTask).Name + " not run yet.";

            Status = JsonConvert.SerializeObject(new
            {
                status = message
            }, _serializer);

            QuoteOfDay = JsonConvert.DeserializeObject<UserQuoteOfDay>
                (File.ReadAllText(_output));
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_ioc.ContextStatus == ContextType.UnitTest)
                QuoteOfDay = JsonConvert.DeserializeObject<UserQuoteOfDay>
                    (File.ReadAllText(_output));
            else
                DoWork(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_delay), cancellationToken);

                DoWork(cancellationToken);
            }
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                var response = _client.GetAsync(_url, cancellationToken).Result;
                var quotes = JsonConvert.DeserializeObject<UserQuoteOfDay>(response.Content.ReadAsStringAsync().Result);

                if (response.IsSuccessStatusCode)
                {
                    if (QuoteOfDay == null
                        || QuoteOfDay.contents.quotes[0].id != quotes.contents.quotes[0].id)
                    {
                        QuoteOfDay = quotes;

                        File.WriteAllText(_output, JsonConvert.SerializeObject(QuoteOfDay));

                        var message = typeof(MaintainQuotesTask).Name + " success on " + DateTime.Now.ToString();

                        Status = JsonConvert.SerializeObject(new
                        {
                            status = message
                        }, _serializer);

                        Log.Information(message);
                    }
                }
                else
                {
                    var message = typeof(MaintainQuotesTask).Name + " fail on " + DateTime.Now.ToString();

                    Status = JsonConvert.SerializeObject(new
                    {
                        status = message,
                        request = response.RequestMessage.ToString(),
                        response = response.ToString()
                    }, _serializer);

                    Log.Error(message 
                        + Environment.NewLine + response.RequestMessage.ToString() 
                        + Environment.NewLine + response.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
