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
using System.Threading.Tasks;
using Twilio.Exceptions;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class DequeueTextsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;
        private const string _callPath = "DequeueTextsJob.Execute";

        public DequeueTextsJob(IServiceScopeFactory factory) => _factory = factory;

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
                    var twilio = scope.ServiceProvider.GetRequiredService<ITwilioService>();

                    await DoDequeueWork(conf, uow, twilio);
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

        private async ValueTask DoDequeueWork(IConfiguration conf, IUnitOfWork uow, ITwilioService twilio)
        {
            var twilioSid = conf["Jobs:DequeueTexts:TwilioSid"];
            var twilioToken = conf["Jobs:DequeueTexts:TwilioToken"];

            foreach (var msg in uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_TextQueue>()
                .Where(x => x.SendAtUtc < DateTime.UtcNow && x.DeliveredUtc.HasValue == false).ToLambda()))
            {
                switch (uow.InstanceType)
                {
                    case InstanceContext.DeployedOrLocal:
                    case InstanceContext.End2EndTest:
                        {
#if RELEASE
                            try
                            {
                                var response = await twilio.TryTextHandoff(twilioSid, twilioToken, msg);

                                uow.TextActivity.Create(
                                    new tbl_TextActivity()
                                    {
                                        Id = Guid.NewGuid(),
                                        TextId = msg.Id,
                                        TwilioSid = response.Sid,
                                        TwilioStatus = response.Status.ToString(),
                                        StatusAtUtc = DateTime.UtcNow,
                                    });

                                msg.DeliveredUtc = DateTime.UtcNow;
                                uow.TextQueue.Update(msg);

                                Log.Information($"'{_callPath}' hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            catch (ApiException ex)
                            {
                                uow.TextActivity.Create(
                                    new tbl_TextActivity()
                                    {
                                        Id = Guid.NewGuid(),
                                        TextId = msg.Id,
                                        TwilioStatus = ex.Code.ToString(),
                                        TwilioMessage = ex.Message,
                                        StatusAtUtc = DateTime.UtcNow,
                                    });

                                Log.Warning($"'{_callPath}' hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                        "Error=" + ex.Code.ToString());
                            }
#elif !RELEASE
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.TextQueue.Update(msg);

                            Log.Information($"'{_callPath}' fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                        }
                        break;

                    default:
                        {
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.TextQueue.Update(msg);

                            Log.Information($"'{_callPath}' fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
                        }
                        break;
                }

                uow.Commit();
            }
        }
    }
}
