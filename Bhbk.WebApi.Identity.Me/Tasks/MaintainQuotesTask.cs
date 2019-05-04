using Bhbk.Lib.Core.Primitives.Enums;
using Bhbk.Lib.Identity.Internal.Infrastructure;
using Bhbk.Lib.Identity.Internal.Models;
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
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly string _url = string.Empty, _output = string.Empty;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainQuotesTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _delay = int.Parse(conf["Tasks:MaintainQuotes:PollingDelay"]);
            _url = conf["Tasks:MaintainQuotes:QuoteOfDayUrl"];
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainQuotesTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), stoppingToken);

                try
                {
                    /*
                     * async database calls from background services should be
                     * avoided so threading issues do not occur.
                     * 
                     * when calling scoped service (unit of work) from a singleton
                     * service (background task) wrap in using block to mimic transient.
                     */

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        if (uow.InstanceType == InstanceContext.DeployedOrLocal)
                        {
                            var motdtype1_response = new HttpClient().GetAsync(_url, stoppingToken).Result;
                            var motdtype1 = JsonConvert.DeserializeObject<MotDType1Response>(motdtype1_response.Content.ReadAsStringAsync().Result);

                            if (motdtype1_response.IsSuccessStatusCode)
                            {
                                var model = uow.Mapper.Map<tbl_MotD_Type1>(motdtype1.contents.quotes[0]);
                                var motd = uow.UserRepo.GetMOTDAsync(x => x.Author == model.Author
                                    && x.Quote == model.Quote).Result.SingleOrDefault();

                                if (motd == null)
                                {
                                    /*
                                     * parts of model are broken and need be fixed...
                                     */
                                    if (model.Id == null)
                                        model.Id = Guid.NewGuid().ToString();

                                    uow.UserRepo.CreateMOTDAsync(model).Wait();
                                    uow.CommitAsync().Wait();
                                }
                                else if (motd.Id != model.Id)
                                {
                                    motd.Id = model.Id;

                                    uow.UserRepo.CreateMOTDAsync(motd).Wait();
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
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
            }
        }
    }
}
