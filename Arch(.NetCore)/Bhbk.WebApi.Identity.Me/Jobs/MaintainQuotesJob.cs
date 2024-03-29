﻿using AutoMapper;
using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
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

namespace Bhbk.WebApi.Identity.Me.Jobs
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
                            var result = JsonConvert.DeserializeObject<MOTDTssV1Response>(response.Content.ReadAsStringAsync().Result);

                            if (response.IsSuccessStatusCode)
                                foreach (var quote in result.contents.quotes)
                                    ProcessMOTDSuccess(uow, map, quote);
                            else
                                ProcessMOTDFail(response);
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

        private static void ProcessMOTDFail(HttpResponseMessage response)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            Log.Error($"'{callPath}' fail on " + DateTime.UtcNow.ToString()
                + Environment.NewLine + response.Content.ReadAsStringAsync().ConfigureAwait(false)
                + Environment.NewLine + response.ToString());
        }

        private static void ProcessMOTDSuccess(IUnitOfWork uow, IMapper map, MOTDTssV1 quote)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var model = map.Map<uvw_MOTD>(quote);

            var motds = uow.MOTDs.Get(QueryExpressionFactory.GetQueryExpression<uvw_MOTD>()
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
                    Log.Error($"'{callPath}' fail adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
                }
                else
                {
                    uow.MOTDs.Create(model);
                    uow.Commit();

                    Log.Information($"'{callPath}' success adding. Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
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

                    Log.Warning($"'{callPath}' success updating non-key(s). Author:\"{model.Author}\" Quote:\"{model.Quote}\"");
                }
            }
            else
                throw new NotImplementedException();
        }
    }
}
