using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Providers
{
    public class QuoteOfDayProvider
    {
        private const string _qodUrl = "https://quotes.rest/qod/";
        private readonly HttpClient _client;

        public QuoteOfDayProvider()
        {
            _client = new HttpClient();
        }

        public async Task UpdateAsync(CancellationToken cancellationToken)
        {
            try
            {
                var qod = await _client.GetAsync(_qodUrl, cancellationToken);

                if (qod.IsSuccessStatusCode)
                    Result = JsonConvert.DeserializeObject<QuoteOfDayResult>(await qod.Content.ReadAsStringAsync());
                else
                    throw new ArgumentNullException();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, BaseLib.Statics.MsgSystemExceptionCaught);
            }
        }

        public QuoteOfDayResult Result { get; private set; } = new QuoteOfDayResult();
    }

    public class QuoteOfDayResult
    {
        public Success success { get; set; }
        public Contents contents { get; set; }

        public class Contents
        {
            public List<Quote> quotes { get; set; }
        }

        public class Quote
        {
            public string quote { get; set; }
            public string length { get; set; }
            public string author { get; set; }
            public List<string> tags { get; set; }
            public string category { get; set; }
            public string date { get; set; }
            public string title { get; set; }
            public string background { get; set; }
            public string id { get; set; }
        }

        public class Success
        {
            public int total { get; set; }
        }
    }
}
