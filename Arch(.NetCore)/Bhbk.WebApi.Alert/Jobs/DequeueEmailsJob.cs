using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Bhbk.WebApi.Alert.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
#if RELEASE
using System.Net;
#endif
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class DequeueEmailsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;
        private const string _callPath = "DequeueEmailsJob.Execute";

        public DequeueEmailsJob(IServiceScopeFactory factory) => _factory = factory;

        public async Task Execute(IJobExecutionContext context)
        {
#if !RELEASE
            Log.Information($"'{_callPath}' running");
#endif
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var sendgrid = scope.ServiceProvider.GetRequiredService<ISendgridService>();

                    await DoDequeueWork(conf, uow, sendgrid);
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
#if !RELEASE
            Log.Information($"'{_callPath}' completed");
            Log.Information($"'{_callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");
#endif
        }

        private async ValueTask DoDequeueWork(IConfiguration conf, IUnitOfWork uow, ISendgridService sendgrid)
        {
            var sendgridApiKey = conf["Jobs:DequeueEmails:SendgridApiKey"];

            foreach (var msg in uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>()
                .Where(x => x.SendAtUtc < DateTime.UtcNow && x.DeliveredUtc.HasValue == false).ToLambda()))
            {
                switch (uow.InstanceType)
                {
                    case InstanceContext.DeployedOrLocal:
                    case InstanceContext.End2EndTest:
                        {
#if RELEASE
                            var response = await sendgrid.TryEmailHandoff(sendgridApiKey, msg);

                            if (response.StatusCode == HttpStatusCode.Accepted)
                            {
                                uow.EmailActivity.Create(
                                    new tbl_EmailActivity()
                                    {
                                        Id = Guid.NewGuid(),
                                        EmailId = msg.Id,
                                        SendgridId = response.Headers.GetValues("X-MESSAGE-ID").Single(),
                                        SendgridStatus = response.StatusCode.ToString(),
                                        StatusAtUtc = DateTime.UtcNow,
                                    });

                                msg.DeliveredUtc = DateTime.UtcNow;
                                uow.EmailQueue.Update(msg);

                                Log.Information($"'{_callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            else
                            {
                                uow.EmailActivity.Create(
                                    new tbl_EmailActivity()
                                    {
                                        Id = Guid.NewGuid(),
                                        EmailId = msg.Id,
                                        SendgridStatus = response.StatusCode.ToString(),
                                        StatusAtUtc = DateTime.UtcNow,
                                    });

                                Log.Warning($"'{_callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                        "Error=" + response.StatusCode);
                            }
#elif !RELEASE
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.EmailQueue.Update(msg);

                            Log.Information($"'{_callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                        }
                        break;

                    default:
                        {
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.EmailQueue.Update(msg);

                            Log.Information($"'{_callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
                        }
                        break;
                }

                uow.Commit();
            }
        }
    }
}
