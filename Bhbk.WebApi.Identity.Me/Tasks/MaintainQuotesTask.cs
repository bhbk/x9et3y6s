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
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Tasks
{
    public class MaintainQuotesTask : BackgroundService
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings-api.json");
        private readonly FileInfo _qf = FileSystemHelper.SearchPaths("appquotes.json");
        private readonly HttpClient _client = new HttpClient();
        private readonly int _interval;
        private readonly string _url;
        public UserQuoteOfDay QuoteOfDay { get; private set; }
        public string Status { get; private set; }

        public MaintainQuotesTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _interval = int.Parse(_cb["Tasks:MaintainQuotes:PollingInterval"]);
            _url = _cb["Tasks:MaintainQuotes:QuoteOfDayUrl"];
            _ioc = ioc;

            Status = string.Empty;
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (_ioc.ContextStatus == ContextType.UnitTest)
                QuoteOfDay = JsonConvert.DeserializeObject<UserQuoteOfDay>
                    (File.ReadAllText(_qf.DirectoryName + Path.DirectorySeparatorChar + _qf.Name));
            else
                DoWork(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                DoWork(cancellationToken);
            }
        }

        private void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                var result = _client.GetAsync(_url, cancellationToken).Result;
                var quote = JsonConvert.DeserializeObject<UserQuoteOfDay>(result.Content.ReadAsStringAsync().Result);

                if (result.IsSuccessStatusCode)
                {
                    if(quote.contents.quotes[0].id != QuoteOfDay.contents.quotes[0].id)
                    {
                        QuoteOfDay = quote;

                        File.WriteAllText(_qf.DirectoryName + Path.DirectorySeparatorChar + _qf.Name, JsonConvert.SerializeObject(QuoteOfDay));
                        Log.Information("Ran " + typeof(MaintainQuotesTask).Name + " in background. Update quote of the day.");
                    }
                }

                Status = result.ReasonPhrase;
            }
            catch (Exception ex)
            {
                Log.Error(ex, BaseLib.Statics.MsgSystemExceptionCaught);
            }
        }
    }
}
