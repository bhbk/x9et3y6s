using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainEmailsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainEmailsJob(IServiceScopeFactory factory) => _factory = factory;

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

                    DoCleanupWork(conf, uow);
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

        private static void DoCleanupWork(IConfiguration conf, IUnitOfWork uow)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var expire = int.Parse(conf["Jobs:MaintainEmails:ExpireDelay"]);

            foreach (var entry in uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>()
                .Where(x => x.CreatedUtc < DateTime.UtcNow.AddSeconds(-(expire))).ToLambda()))
            {
                Log.Warning($"'{callPath}' is deleting email (ID=" + entry.Id.ToString() + ") that was created on " 
                    + entry.CreatedUtc.ToLocalTime());

                uow.EmailQueue.Delete(entry);
            }

            uow.Commit();
        }
    }
}
