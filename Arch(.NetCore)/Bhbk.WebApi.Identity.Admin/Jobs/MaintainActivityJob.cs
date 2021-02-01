using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
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
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var auditable = int.Parse(conf["Jobs:MaintainActivity:HoldAuditable"]);
                    var transient = int.Parse(conf["Jobs:MaintainActivity:HoldTransient"]);

                    var expiredExpr = QueryExpressionFactory.GetQueryExpression<tbl_AuthActivity>()
                        .Where(x => (x.CreatedUtc.AddSeconds(transient) < DateTime.UtcNow)
                            || (x.CreatedUtc.AddSeconds(auditable) < DateTime.UtcNow)).ToLambda();

                    var expired = uow.AuthActivity.Get(expiredExpr);
                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        uow.AuthActivity.Delete(expired);
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
