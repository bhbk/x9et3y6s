using Bhbk.Lib.Identity.Data.Infrastructure_Tbl;
using Bhbk.Lib.Identity.Data.Models_Tbl;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainStatesJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainStatesJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var invalidExpr = QueryExpressionFactory.GetQueryExpression<tbl_State>()
                            .Where(x => x.ValidFromUtc > DateTime.UtcNow || x.ValidToUtc < DateTime.UtcNow).ToLambda();

                    var invalid = uow.States.Get(invalidExpr);
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        uow.States.Delete(invalid);
                        uow.Commit();

                        Log.Information($"'{callPath}' success on " + DateTime.UtcNow.ToString() + ". Delete "
                            + invalidCount.ToString() + " invalid states.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
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
