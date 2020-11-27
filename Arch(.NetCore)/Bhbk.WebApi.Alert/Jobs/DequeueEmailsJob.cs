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
#if RELEASE
using System.Net;
#endif
using System.Threading.Tasks;
using System.Reflection;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class DequeueEmailsJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public DequeueEmailsJob(IServiceScopeFactory factory) => _factory = factory;

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
                    var sendgrid = scope.ServiceProvider.GetRequiredService<ISendgridService>();

                    DoDequeueWork(conf, uow, sendgrid);
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

        private void DoDequeueWork(IConfiguration conf, IUnitOfWork uow, ISendgridService sendgrid)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
            var sendgridApiKey = conf["Jobs:DequeueEmails:SendgridApiKey"];

            foreach (var msg in uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<uvw_EmailQueue>()
                .Where(x => x.SendAtUtc < DateTime.UtcNow 
                    && x.DeliveredUtc.HasValue == false && x.IsCancelled == false).ToLambda()))
            {
                switch (uow.InstanceType)
                {
                    case InstanceContext.DeployedOrLocal:
                    case InstanceContext.End2EndTest:
                        {
#if RELEASE
                            var response = sendgrid.TryEmailHandoff(sendgridApiKey, msg).Result;

                            if (response.StatusCode == HttpStatusCode.Accepted)
                            {
                                uow.EmailActivities.Create(
                                    new uvw_EmailActivity()
                                    {
                                        EmailId = msg.Id,
                                        SendgridId = response.Headers.GetValues("X-MESSAGE-ID").Single(),
                                        SendgridStatus = response.StatusCode.ToString(),
                                    });

                                msg.DeliveredUtc = DateTime.UtcNow;
                                uow.EmailQueue.Update(msg);

                                Log.Information($"'{callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            else
                            {
                                uow.EmailActivities.Create(
                                    new uvw_EmailActivity()
                                    {
                                        EmailId = msg.Id,
                                        SendgridStatus = response.StatusCode.ToString(),
                                    });

                                Log.Warning($"'{callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                        "Error=" + response.StatusCode);
                            }
#elif !RELEASE
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.EmailQueue.Update(msg);

                            Log.Information($"'{callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                        }
                        break;

                    default:
                        {
                            msg.DeliveredUtc = DateTime.UtcNow;
                            uow.EmailQueue.Update(msg);

                            Log.Information($"'{callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
                        }
                        break;
                }

                uow.Commit();
            }
        }
    }
}
