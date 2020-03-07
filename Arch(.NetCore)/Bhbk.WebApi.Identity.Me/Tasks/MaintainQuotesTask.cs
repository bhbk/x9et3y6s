using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Me.Tasks
{
    public class MaintainQuotesTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly string _key = string.Empty, _url = string.Empty;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainQuotesTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _delay = int.Parse(conf["Tasks:MaintainQuotes:PollingDelay"]);
            _key = conf["Tasks:MaintainQuotes:TheySaidSoApiKey"];
            _url = conf["Tasks:MaintainQuotes:TheySaidSoApiUrl"];
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

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if(!string.IsNullOrEmpty(_key))
                    await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);
                else
                    await Task.Delay((TimeSpan.FromSeconds(_delay) * 60), cancellationToken);

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
                            if (!string.IsNullOrEmpty(_key))
                            {
                                using (var http = new HttpClient())
                                {
                                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                    http.DefaultRequestHeaders.Add("X-TheySaidSo-Api-Secret", _key);

                                    var response = await http.GetAsync(_url + "/quote/random.json?language=en&limit=10", cancellationToken);
                                    var results = JsonConvert.DeserializeObject<MOTDTssV1Response>(await response.Content.ReadAsStringAsync());

                                    if (response.IsSuccessStatusCode)
                                        foreach (var quote in results.contents.quotes)
                                            ProcessMOTDSuccess(uow, mapper, quote);
                                    else
                                        ProcessMOTDFail(response);
                                }
                            }
                            else
                            {
                                using (var http = new HttpClient())
                                {
                                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    var response = await http.GetAsync(_url + "/qod.json", cancellationToken);
                                    var results = JsonConvert.DeserializeObject<MOTDTssV1Response>(await response.Content.ReadAsStringAsync());

                                    if (response.IsSuccessStatusCode)
                                        ProcessMOTDSuccess(uow, mapper, results.contents.quotes[0]);
                                    else
                                        ProcessMOTDFail(response);
                                }
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

        private void ProcessMOTDFail(HttpResponseMessage response)
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

        private void ProcessMOTDSuccess(IUnitOfWork uow, IMapper mapper, MOTDTssV1 quote)
        {
            string msg = string.Empty;
            var model = mapper.Map<tbl_MOTDs>(quote);

            var motds = uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<tbl_MOTDs>()
                .Where(x => x.Author == model.Author && x.Quote == model.Quote)
                .ToLambda());

            if (!motds.Any())
            {
                /*
                 * parts of model are broken...
                 */

                if (string.IsNullOrEmpty(model.Author)
                    || string.IsNullOrEmpty(model.Quote))
                {
                    msg = $"{typeof(MaintainQuotesTask).Name} fail adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"";
                }
                else
                {
                    uow.MOTDs.Create(model);
                    uow.Commit();

                    msg = $"{typeof(MaintainQuotesTask).Name} success adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"";
                }
            }
            else if (motds.Count() == 1)
            {
                var motd = motds.Single();
                var dirty = false;

                /*
                 * parts of model are broken and need be fixed...
                 */

                if (motd.TssId != model.TssId)
                {
                    motd.TssId = model.TssId;
                    dirty = true;
                }

                if (motd.TssTitle != model.TssTitle)
                {
                    motd.TssTitle = model.TssTitle;
                    dirty = true;
                }

                if (motd.TssCategory != model.TssCategory)
                {
                    motd.TssCategory = model.TssCategory;
                    dirty = true;
                }

                if (motd.TssDate != model.TssDate)
                {
                    motd.TssDate = model.TssDate;
                    dirty = true;
                }

                if (motd.TssTags != model.TssTags)
                {
                    motd.TssTags = model.TssTags;
                    dirty = true;
                }

                if (motd.TssBackground != model.TssBackground)
                {
                    motd.TssBackground = model.TssBackground;
                    dirty = true;
                }

                if (dirty)
                {
                    uow.MOTDs.Update(motd);
                    uow.Commit();

                    msg = $"{typeof(MaintainQuotesTask).Name} success updating non-key(s). Author:\"{model.Author}\" Quote:\"{model.Quote}\"";
                }
            }
            else
                throw new NotImplementedException();

            if(!string.IsNullOrEmpty(msg))
            {
                Status = JsonConvert.SerializeObject(
                    new
                    {
                        status = msg
                    }, _serializer);

                Log.Information(msg);
            }
        }
    }
}
