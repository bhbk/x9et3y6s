using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_TBL;
using Bhbk.Lib.Identity.Data.EFCore.Models_TBL;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainUsersJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainUsersJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var disabledExpr = QueryExpressionFactory.GetQueryExpression<tbl_User>()
                            .Where(x => x.LockoutEndUtc < DateTime.UtcNow).ToLambda();

                    var disabled = uow.Users.Get(disabledExpr);
                    var disabledCount = disabled.Count();

                    if (disabled.Any())
                    {
                        uow.Users.Delete(disabled);
                        uow.Commit();

                        Log.Information($"'{callPath}' success on " + DateTime.UtcNow.ToString() + ". Enabled "
                            + disabledCount.ToString() + " users with expired lock-outs.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            finally
            {
                GC.Collect();
            }

            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");

            return Task.CompletedTask;
        }
    }
}
