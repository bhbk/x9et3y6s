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
    public class EmailActivityJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public EmailActivityJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    DoCleanupWork(uow);
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

        private static void DoCleanupWork(IUnitOfWork uow)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var job = uow.Jobs.Get(x => x.Name == "Email Activity").Single();
            var expire = int.Parse(uow.JobSettings.Get(x => x.JobId == job.Id && x.ConfigKey == "ExpireDelay").Single().ConfigValue);

            foreach (var entry in uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>()
                .Where(x => x.Created < DateTime.UtcNow.AddSeconds(-(expire))).ToLambda()))
            {
                Log.Warning($"'{callPath}' is deleting email (ID=" + entry.Id.ToString() + ") that was created on "
                    + entry.Created.ToLocalTime());

                uow.EmailQueue.Delete(entry);
            }

            uow.Commit();
        }
    }
}
