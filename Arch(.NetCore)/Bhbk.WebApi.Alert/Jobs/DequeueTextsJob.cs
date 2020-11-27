using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.Infrastructure;
using Bhbk.Lib.Identity.Data.Models;
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
using System.Reflection;
#if RELEASE
using Twilio.Exceptions;
#endif

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class DequeueTextsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public DequeueTextsJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
#if !RELEASE
            Log.Information($"'{callPath}' running");
#endif
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var twilio = scope.ServiceProvider.GetRequiredService<ITwilioService>();

                    DoDequeueWork(conf, uow, twilio);
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
            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");
#endif
            return Task.CompletedTask;
        }

        private void DoDequeueWork(IConfiguration conf, IUnitOfWork uow, ITwilioService twilio)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var twilioSid = conf["Jobs:DequeueTexts:TwilioSid"];
            var twilioToken = conf["Jobs:DequeueTexts:TwilioToken"];

            foreach (var msg in uow.TextQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_TextQueue>()
                .Where(x => x.SendAtUtc < DateTime.UtcNow
                    && x.DeliveredUtc.HasValue == false && x.IsCancelled == false).ToLambda()))
            {
                switch (uow.InstanceType)
                {
                    case InstanceContext.DeployedOrLocal:
                    case InstanceContext.End2EndTest:
                        {
#if RELEASE
                            try
                            {
                                var response = twilio.TryTextHandoff(twilioSid, twilioToken, msg).Result;

                                uow.TextActivities.Create(
                                    new uvw_TextActivity()
                                    {
                                        TextId = msg.Id,
                                        TwilioSid = response.Sid,
                                        TwilioStatus = response.Status.ToString(),
                                    });

                                msg.DeliveredUtc = DateTime.UtcNow;
                                uow.TextQueue.Update(msg);

                                Log.Information($"'{callPath}' hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            catch (ApiException ex)
                            {
                                uow.TextActivities.Create(
                                    new uvw_TextActivity()
                                    {
                                        TextId = msg.Id,
                                        TwilioStatus = ex.Code.ToString(),
                                        TwilioMessage = ex.Message,
                                    });

                                Log.Warning($"'{callPath}' hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                        "Error=" + ex.Code.ToString());
                            }
#elif !RELEASE
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.TextQueue.Update(msg);

                            Log.Information($"'{callPath}' fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                        }
                        break;

                    default:
                        {
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.TextQueue.Update(msg);

                            Log.Information($"'{callPath}' fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
                        }
                        break;
                }

                uow.Commit();
            }
        }
    }
}
