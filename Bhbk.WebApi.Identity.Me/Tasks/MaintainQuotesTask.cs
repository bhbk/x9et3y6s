using Bhbk.Lib.Identity.Factory;
using Bhbk.Lib.Identity.Helpers;
using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Microsoft.Extensions.Configuration;
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
    public class MaintainQuotesTask : CustomIdentityTask
    {
        private readonly IIdentityContext _ioc;
        private readonly IConfigurationRoot _cb;
        private readonly FileInfo _cf = FileSystemHelper.SearchPaths("appsettings.json");
        private readonly FileInfo _qf = FileSystemHelper.SearchPaths("appquotes.json");
        private readonly HttpClient _client = new HttpClient();
        private readonly int _interval;
        private string _api;

        public MaintainQuotesTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _cb = new ConfigurationBuilder()
                .SetBasePath(_cf.DirectoryName)
                .AddJsonFile(_cf.Name, optional: false, reloadOnChange: true)
                .Build();

            _ioc = ioc;
            _interval = int.Parse(_cb["Tasks:MaintainQuotes:PollingInterval"]);
            _api = _cb["Tasks:MaintainQuotes:ApiGetQoDUrl"];
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _ioc.UserQuote = JsonConvert.DeserializeObject<UserQuoteOfDay>
                (File.ReadAllText(_qf.DirectoryName + Path.DirectorySeparatorChar + _qf.Name));

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(_interval), cancellationToken);

                    Log.Information("Task " + typeof(MaintainQuotesTask).Name + ". Updated quote of the day.");

                    var result = await _client.GetAsync(_api, cancellationToken);

                    if (result.IsSuccessStatusCode)
                        _ioc.UserQuote = JsonConvert.DeserializeObject<UserQuoteOfDay>(await result.Content.ReadAsStringAsync());

                    Status = result.ReasonPhrase;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }

            File.WriteAllText(_qf.DirectoryName + Path.DirectorySeparatorChar + _qf.Name, JsonConvert.SerializeObject(_ioc.UserQuote));
        }

        public string Status { get; private set; }
    }
}
