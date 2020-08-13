using Bhbk.Lib.Common.Primitives.Enums;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Bhbk.WebApi.Alert.Helpers;
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
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class QueueEmailJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public QueueEmailJob(IServiceScopeFactory factory) => _factory = factory;

        public Task Execute(IJobExecutionContext context)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";
#if DEBUG
            Log.Information($"'{callPath}' running");
#endif
            try
            {
                using (var scope = _factory.CreateScope())
                {
                    var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var enable = bool.Parse(conf["Jobs:QueueEmails:Enable"]);
                    var expire = int.Parse(conf["Jobs:QueueEmails:ExpireDelay"]);
                    var providerApiKey = conf["Jobs:QueueEmails:ProviderApiKey"];

                    foreach (var entry in uow.QueueEmails.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueEmails>()
                        .Where(x => x.Created < DateTime.Now.AddSeconds(-(expire))).ToLambda()))
                    {
                        Log.Warning(callPath + " hand-off of email (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                            "The email was created on " + entry.Created + " and is being deleted now.");

                        uow.QueueEmails.Delete(entry);
                    }

                    uow.Commit();

                    var provider = new SendGridProvider();

                    foreach (var msg in uow.QueueEmails.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueEmails>()
                        .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                    {
                        switch (uow.InstanceType)
                        {
                            case InstanceContext.DeployedOrLocal:
                                {
#if RELEASE
                                    var response = provider.TryEmailHandoff(providerApiKey, msg).Result;

                                    if (response.StatusCode == HttpStatusCode.Accepted)
                                    {
                                        uow.QueueEmails.Delete(msg);
                                        Log.Information(callPath + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                                    }
                                    else
                                        Log.Warning(callPath + " hand-off of email (ID=" + msg.Id.ToString() + ") to upstream provider failed. " +
                                            "Error=" + response.StatusCode);
#elif !RELEASE
                                    uow.QueueEmails.Delete(msg);
                                    Log.Information(callPath + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                }
                                break;

                            case InstanceContext.End2EndTest:
                            case InstanceContext.IntegrationTest:
                                {
                                    uow.QueueEmails.Delete(msg);
                                    Log.Information(callPath + " fake hand-off of email (ID=" + msg.Id.ToString() + ") was successfull.");
                                }
                                break;

                            default:
                                throw new NotImplementedException();
                        }
                    }

                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            GC.Collect();
#if DEBUG
            Log.Information($"'{callPath}' completed");
            Log.Information($"'{callPath}' will run again at {context.NextFireTimeUtc.Value.LocalDateTime}");
#endif

            return Task.CompletedTask;
        }
    }
}
