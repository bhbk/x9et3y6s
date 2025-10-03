using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.Identity.Models.Me;
using Bhbk.Lib.Identity.Primitives.Constants;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.User.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainQuotesJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainQuotesJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var map = scope.ServiceProvider.GetRequiredService<IMapper>();

                    if (uow.InstanceType == InstanceContext.DeployedOrLocal
                        || uow.InstanceType == InstanceContext.End2EndTest)
                    {
                        var url = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                            && x.ConfigKey == SettingsConstants.TheySaidSoUrl).Single();

                        var key = uow.Settings.Get(x => x.IssuerId == null && x.AudienceId == null && x.UserId == null
                            && x.ConfigKey == SettingsConstants.TheySaidSoLicense).Single();

                        using (var http = new HttpClient())
                        {
                            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            http.DefaultRequestHeaders.Add("X-TheySaidSo-Api-Secret", key.ConfigValue);

                            var response = http.GetAsync(url.ConfigValue + "/quote/random.json?language=en").Result;
                            var result = JsonConvert.DeserializeObject<QuoteV1Response>(response.Content.ReadAsStringAsync().Result);

                            if (response.IsSuccessStatusCode)
                                foreach (var quote in result.contents.quotes)
                                    ProcessQuoteSuccess(uow, map, quote);
                            else
                                ProcessQuoteFail(response);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 Log.Fatal(ex, $"'{callPath}' failed on {Dns.GetHostName().ToUpper()}");
            }
            finally
            {
                GC.Collect();
            }

            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.GetValueOrDefault().LocalDateTime}");

            return Task.CompletedTask;
        }

        private static void ProcessQuoteFail(HttpResponseMessage response)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            Log.Error($"'{callPath}' fail on " + DateTime.UtcNow.ToString()
                + Environment.NewLine + response.Content.ReadAsStringAsync().ConfigureAwait(false)
                + Environment.NewLine + response.ToString());
        }

        private static void ProcessQuoteSuccess(IUnitOfWork uow, IMapper map, QuoteV1 quote)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var model = map.Map<tbl_Quote>(quote);

            var entries = uow.Quotes.Get(QueryExpressionFactory.GetQueryExpression<tbl_Quote>()
                .Where(x => x.Author == model.Author && x.Quote == model.Quote)
                .ToLambda());

            if (!entries.Any())
            {
                /*
                 * parts of model are broken...
                 */
                if (string.IsNullOrEmpty(model.Author)
                    || string.IsNullOrEmpty(model.Quote))
                {
                    Log.Error($"'{callPath}' fail adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
                }
                else
                {
                    uow.Quotes.Create(model);
                    uow.Commit();

                    Log.Information($"'{callPath}' success adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
                }
            }
            else if (entries.Count() == 1)
            {
                var entry = entries.Single();
                var dirty = false;

                /*
                 * parts of model are broken and need be fixed...
                 */
                if (entry.TssId != model.TssId)
                {
                    entry.TssId = model.TssId;
                    dirty = true;
                }

                if (entry.TssTitle != model.TssTitle)
                {
                    entry.TssTitle = model.TssTitle;
                    dirty = true;
                }

                if (entry.TssCategory != model.TssCategory)
                {
                    entry.TssCategory = model.TssCategory;
                    dirty = true;
                }

                if (entry.TssDate != model.TssDate)
                {
                    entry.TssDate = model.TssDate;
                    dirty = true;
                }

                if (entry.TssTags != model.TssTags)
                {
                    entry.TssTags = model.TssTags;
                    dirty = true;
                }

                if (entry.TssBackground != model.TssBackground)
                {
                    entry.TssBackground = model.TssBackground;
                    dirty = true;
                }

                if (dirty)
                {
                    uow.Quotes.Update(entry);
                    uow.Commit();

                    Log.Warning($"'{callPath}' success updating non-key(s). Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
                }
            }
            else
                throw new NotImplementedException();
        }
    }
}
