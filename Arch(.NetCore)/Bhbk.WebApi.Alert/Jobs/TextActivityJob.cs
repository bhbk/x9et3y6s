using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
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

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class TextActivityJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public TextActivityJob(IServiceScopeFactory factory) => _factory = factory;

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

        private static void DoCleanupWork(IConfiguration conf, IUnitOfWork uow)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var expire = int.Parse(conf["Jobs:TextActivity:ExpireDelay"]);

            foreach (var entry in uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>()
                .Where(x => x.CreatedUtc < DateTime.UtcNow.AddSeconds(-(expire))).ToLambda()))
            {
                Log.Warning($"'{callPath}' is deleting text (ID=" + entry.Id.ToString() + ") that was created on "
                    + entry.CreatedUtc.ToLocalTime());

                uow.TextQueue.Delete(entry);
            }

            uow.Commit();
        }
    }
}
