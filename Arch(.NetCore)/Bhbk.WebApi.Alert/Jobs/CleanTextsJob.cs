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
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class CleanTextsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;
        private const string _callPath = "CleanTextsJob.Execute";

        public CleanTextsJob(IServiceScopeFactory factory) => _factory = factory;

        public async Task Execute(IJobExecutionContext context)
        {
            Log.Information($"'{_callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    DoCleanWork(conf, uow);
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

            Log.Information($"'{_callPath}' completed");
            Log.Information($"'{_callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");
        }

        private void DoCleanWork(IConfiguration conf, IUnitOfWork uow)
        {
            var expire = int.Parse(conf["Jobs:CleanTexts:ExpireDelay"]);

            foreach (var entry in uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>()
                .Where(x => x.CreatedUtc < DateTime.UtcNow.AddSeconds(-(expire))).ToLambda()))
            {
                Log.Warning($"'{_callPath}' is deleting text (ID=" + entry.Id.ToString() + ") that was created on "
                    + entry.CreatedUtc.ToLocalTime());

                uow.TextQueue.Delete(entry);
            }

            uow.Commit();
        }
    }
}
