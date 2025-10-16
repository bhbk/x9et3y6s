using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainChatFilesJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainChatFilesJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            Log.Information($"'{callPath}' running");

            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var expired = uow.ChatFiles.Get(x => x.ExpiresUtc < DateTimeOffset.UtcNow);
                    var expiredCount = expired.Count();

                    if (expired.Any())
                    {
                        uow.ChatFiles.Delete(expired);
                        uow.Commit();

                        Log.Information($"'{callPath}' success on " + DateTime.UtcNow.ToString() + ". Delete "
                            + expiredCount.ToString() + " expired chat files.");
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
