using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure;
using Bhbk.Lib.Identity.Data.EFCore.Models;
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
                        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                        if (uow.InstanceType == InstanceContext.DeployedOrLocal)
                        {
                            var motdtype1_response = new HttpClient().GetAsync(_url, stoppingToken).Result;
                            var motdtype1 = JsonConvert.DeserializeObject<MOTDType1Response>(motdtype1_response.Content.ReadAsStringAsync().Result);

                            if (motdtype1_response.IsSuccessStatusCode)
                            {
                                string msg = string.Empty;
                                var model = mapper.Map<tbl_MOTDs>(motdtype1.contents.quotes[0]);

                                var found = uow.MOTDs.Get(new QueryExpression<tbl_MOTDs>()
                                    .Where(x => x.Author == model.Author && x.Quote == model.Quote)
                                    .ToLambda());

                                if (!found.Any())
                                {
                                    /*
                                     * parts of model are missing...
                                     */
                                    if (model.Id == null)
                                        model.Id = Guid.NewGuid().ToString();

                                    uow.MOTDs.Create(model);
                                    uow.Commit();

                                    msg = typeof(MaintainQuotesTask).Name + " success on " + DateTime.Now.ToString() + " with new quote added";
                                }
                                else if (found.Count() == 1)
                                {
                                    var motd = found.Single();
                                    var dirty = false;

                                    /*
                                     * parts of model are broken and need be fixed...
                                     */
                                    if (motd.Id != model.Id)
                                    {
                                        motd.Id = model.Id;
                                        dirty = true;
                                    }

                                    if (motd.Title != model.Title)
                                    {
                                        motd.Title = model.Title;
                                        dirty = true;
                                    }

                                    if (motd.Category != model.Category)
                                    {
                                        motd.Category = model.Category;
                                        dirty = true;
                                    }

                                    if (motd.Date != model.Date)
                                    {
                                        motd.Date = model.Date;
                                        dirty = true;
                                    }

                                    if (motd.Tags != model.Tags)
                                    {
                                        motd.Tags = model.Tags;
                                        dirty = true;
                                    }

                                    if (motd.Background != model.Background)
                                    {
                                        motd.Background = model.Background;
                                        dirty = true;
                                    }

                                    if (dirty)
                                    {
                                        uow.MOTDs.Update(motd);
                                        uow.Commit();

                                        msg = typeof(MaintainQuotesTask).Name + " success on " + DateTime.Now.ToString() + " with existing quote updated";
                                    }
                                }
                                else
                                    throw new NotImplementedException();

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

                /*
                 * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
                 */
                GC.Collect();
            }
        }
    }
}
