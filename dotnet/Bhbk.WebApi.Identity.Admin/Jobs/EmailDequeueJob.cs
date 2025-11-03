using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EF.Infrastructure;
using Bhbk.Lib.Identity.Data.EF.Models;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Bhbk.WebApi.Identity.Admin.Services;
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
    public class EmailDequeueJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public EmailDequeueJob(IServiceScopeFactory factory) => _factory = factory;

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
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var sendgrid = scope.ServiceProvider.GetRequiredService<ISendgridService>();

                    DoDequeueWork(uow, sendgrid);
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
#if !RELEASE
            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.GetValueOrDefault().LocalDateTime}");
#endif
            return Task.CompletedTask;
        }

        private void DoDequeueWork(IUnitOfWork uow, ISendgridService sendgrid)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            var job = uow.Jobs.Get(x => x.Name == "Email Dequeue").Single();
            var sendgridApiKey = uow.JobSettings.Get(x => x.JobId == job.Id && x.ConfigKey == "SendgridApiKey").Single().ConfigValue;

            foreach (var msg in uow.EmailQueue.Get(QueryExpressionFactory.GetQueryExpression<tbl_EmailQueue>()
                .Where(x => x.SendAt < DateTime.UtcNow
                    && x.Delivered.HasValue == false && x.IsCancelled == false).ToLambda()))
            {
                switch (uow.InstanceType)
                {
                    case InstanceContext.DeployedOrLocal:
                    case InstanceContext.SystemTest:
                        {
#if RELEASE
                            var response = sendgrid.TryEmailHandoff(sendgridApiKey, msg).Result;

                            if (response.StatusCode == HttpStatusCode.Accepted)
                            {
                                uow.EmailActivity.Post(
                                    new tbl_EmailActivity()
                                    {
                                        EmailId = msg.Id,
                                        SendgridId = response.Headers.GetValues("X-MESSAGE-ID").Single(),
                                        SendgridStatus = response.StatusCode.ToString(),
                                    });

                                msg.Delivered = DateTime.UtcNow;
                                uow.EmailQueue.Put(msg);

                                Log.Information($"'{callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                            }
                            else
                            {
                                uow.EmailActivity.Post(
                                    new tbl_EmailActivity()
                                    {
                                        EmailId = msg.Id,
                                        SendgridStatus = response.StatusCode.ToString(),
                                    });

                                Log.Warning($"'{callPath}' hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                        "Error=" + response.StatusCode);
                            }
#elif !RELEASE
                            msg.Delivered = DateTime.UtcNow;
                            uow.EmailQueue.Put(msg);

                            Log.Information($"'{callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                        }
                        break;

                    default:
                        {
                            msg.Delivered = DateTime.UtcNow;
                            uow.EmailQueue.Put(msg);

                            Log.Information($"'{callPath}' fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
                        }
                        break;
                }

                uow.Commit();
            }
        }
    }
}
