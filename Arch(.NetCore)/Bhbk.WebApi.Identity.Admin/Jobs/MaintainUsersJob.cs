using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
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
#if DEBUG
            Log.Information($"'{callPath}' running");
#endif
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var disabledExpr = QueryExpressionFactory.GetQueryExpression<tbl_Users>()
                            .Where(x => x.LockoutEnd < DateTime.UtcNow).ToLambda();

                    var disabled = uow.Users.Get(disabledExpr);
                    var disabledCount = disabled.Count();

                    if (disabled.Any())
                    {
                        uow.Users.Delete(disabled);
                        uow.Commit();

                        var msg = callPath + " success on " + DateTime.Now.ToString() + ". Enabled "
                                + disabledCount.ToString() + " users with expired lock-outs.";

                        Log.Information(msg);
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
#if DEBUG
            Log.Information($"'{callPath}' completed");
#endif
            return Task.CompletedTask;
        }
    }
}
