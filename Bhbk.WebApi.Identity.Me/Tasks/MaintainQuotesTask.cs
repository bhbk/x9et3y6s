using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Models;
using Bhbk.Lib.Identity.Internal.UnitOfWork;
using Bhbk.Lib.Identity.Models.Me;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tasks
{
    public class MaintainQuotesTask : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly JsonSerializerSettings _serializer;
        private readonly string _url = string.Empty, _output = string.Empty;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainQuotesTask(IServiceCollection sc)
        {
            if (sc == null)
                throw new ArgumentNullException();

            _sp = sc.BuildServiceProvider();
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            var conf = _sp.GetRequiredService<IConfiguration>();

            _delay = int.Parse(conf["Tasks:MaintainQuotes:PollingDelay"]);
            _url = conf["Tasks:MaintainQuotes:QuoteOfDayUrl"];

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainQuotesTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);

                try
                {
                    var uow = (IIdentityUnitOfWork)_sp.GetRequiredService<IIdentityUnitOfWork>();

                    if (uow.InstanceType == InstanceContext.DeployedOrLocal)
                    {
                        var motdtype1_response = new HttpClient().GetAsync(_url, cancellationToken).Result;
                        var motdtype1 = JsonConvert.DeserializeObject<MotDType1Response>(motdtype1_response.Content.ReadAsStringAsync().Result);

                        if (motdtype1_response.IsSuccessStatusCode)
                        {
                            var model = uow.Mapper.Map<tbl_MotD_Type1>(motdtype1.contents.quotes[0]);
                            var result = uow.UserRepo.GetMOTDAsync(x => x.Author == model.Author 
                                && x.Quote == model.Quote).Result.SingleOrDefault();

                            if(result == null)
                            {
                                /*
                                 * parts of model are broken and need be fixed...
                                 */
                                if (model.Id == null)
                                    model.Id = Guid.NewGuid().ToString();

                                uow.UserRepo.CreateMOTDAsync(model).Wait();
                                uow.CommitAsync().Wait();
                            }

                            var msg = typeof(MaintainQuotesTask).Name + " success on " + DateTime.Now.ToString();

                            Status = JsonConvert.SerializeObject(
                                new
                                {
                                    status = msg
                                }, _serializer);

                            Log.Information(msg);
                        }
                        else
                        {
                            var msg = typeof(MaintainQuotesTask).Name + " fail on " + DateTime.Now.ToString();

                            Status = JsonConvert.SerializeObject(
                                new
                                {
                                    status = msg,
                                    request = motdtype1_response.RequestMessage.ToString(),
                                    response = motdtype1_response.ToString()
                                }, _serializer);

                            Log.Error(msg
                                + Environment.NewLine + motdtype1_response.RequestMessage.ToString()
                                + Environment.NewLine + motdtype1_response.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}
