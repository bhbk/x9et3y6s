﻿using Bhbk.Lib.Core.FileSystem;
using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Interfaces;
using Bhbk.Lib.Primitives.Enums;
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
        private readonly FileInfo _api = Search.DefaultPaths("appsettings-api.json");
        private readonly FileInfo _qod = Search.DefaultPaths("appquotes.json");
        private readonly IConfigurationRoot _conf;
        private readonly IIdentityContext _ioc;
        private readonly JsonSerializerSettings _serializer;
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

            _conf = new ConfigurationBuilder()
                .SetBasePath(_api.DirectoryName)
                .AddJsonFile(_api.Name, optional: false, reloadOnChange: true)
                .Build();

            _output = _qod.DirectoryName + Path.DirectorySeparatorChar + _qod.Name;
            _delay = int.Parse(_conf["Tasks:MaintainQuotes:PollingDelay"]);
            _url = _conf["Tasks:MaintainQuotes:QuoteOfDayUrl"];
            _ioc = ioc;

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainQuotesTask).Name + " not run yet."
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
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

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

                        var msg = typeof(MaintainQuotesTask).Name + " success on " + DateTime.Now.ToString();

                        Status = JsonConvert.SerializeObject(
                            new
                            {
                                status = msg
                            }, _serializer);

                        Log.Information(msg);
                    }
                }
                else
                {
                    var msg = typeof(MaintainQuotesTask).Name + " fail on " + DateTime.Now.ToString();

                    Status = JsonConvert.SerializeObject(
                        new
                        {
                            status = msg,
                            request = response.RequestMessage.ToString(),
                            response = response.ToString()
                        }, _serializer);

                    Log.Error(msg
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
