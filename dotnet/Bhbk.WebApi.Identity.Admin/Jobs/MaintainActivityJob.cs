using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainActivityJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainActivityJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var job = uow.Jobs.Get(x => x.Name == "Maintain Activity").Single();
                    var jobSettings = uow.JobSettings.Get(x => x.JobId == job.Id).ToList();

                    var auditable = int.Parse(jobSettings.Single(x => x.ConfigKey == "HoldAuditable").ConfigValue);
                    var transient = int.Parse(jobSettings.Single(x => x.ConfigKey == "HoldTransient").ConfigValue);

                    var expiredExpr = QueryExpressionFactory.GetQueryExpression<tbl_UserActivity>()
                        .Where(x => (x.Created.AddSeconds(transient) < DateTime.UtcNow)
                            || (x.Created.AddSeconds(auditable) < DateTime.UtcNow)).ToLambda();

                    var expired = uow.UserActivities.Get(expiredExpr);
                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        uow.UserActivities.Delete(expired);
                        uow.Commit();

                        Log.Information($"'{callPath}' success on " + DateTime.UtcNow.ToString() + ". Delete " 
                            + expiredCount.ToString() + " expired activity entries.");
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
    }
}
