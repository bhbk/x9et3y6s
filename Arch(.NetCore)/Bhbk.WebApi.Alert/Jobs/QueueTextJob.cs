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
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Alert.Jobs
{
    [DisallowConcurrentExecution]
    public class QueueTextJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public QueueTextJob(IServiceScopeFactory factory) => _factory = factory;

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

                    var enable = bool.Parse(conf["Jobs:QueueTexts:Enable"]);
                    var expire = int.Parse(conf["Jobs:QueueTexts:ExpireDelay"]);
                    var providerSid = conf["Jobs:QueueTexts:ProviderSid"];
                    var providerToken = conf["Jobs:QueueTexts:ProviderToken"];

                    foreach (var entry in uow.QueueTexts.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueTexts>()
                        .Where(x => x.Created < DateTime.Now.AddSeconds(-(expire))).ToLambda()))
                    {
                        Log.Warning(typeof(QueueTextJob).Name + " hand-off of text (ID=" + entry.Id.ToString() + ") to upstream provider failed many times. " +
                            "The text was created on " + entry.Created + " and is being deleted now.");

                        uow.QueueTexts.Delete(entry);
                    }

                    uow.Commit();

                    var provider = new TwilioProvider();

                    foreach (var msg in uow.QueueTexts.Get(QueryExpressionFactory.GetQueryExpression<tbl_QueueTexts>()
                        .Where(x => x.SendAt < DateTime.Now).ToLambda()))
                    {
                        switch (uow.InstanceType)
                        {
                            case InstanceContext.DeployedOrLocal:
                                {
#if RELEASE
                                    var response = provider.TryTextHandoff(providerSid, providerToken, msg);

                                    uow.QueueTexts.Delete(msg);
                                    Log.Information(callPath + " hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");

                                    //if (response.IsCompletedSuccessfully)
                                    //{
                                    //    uow.QueueTexts.Delete(msg);
                                    //    Log.Information(callPath + " hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider was successfull.");
                                    //}
                                    //else
                                    //    Log.Warning(callPath + " hand-off of text (ID=" + msg.Id.ToString() + ") to upstream provider failed.");
#elif !RELEASE
                                    uow.QueueTexts.Delete(msg);
                                    Log.Information(callPath + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
#endif
                                }
                                break;

                            case InstanceContext.End2EndTest:
                            case InstanceContext.IntegrationTest:
                                {
                                    uow.QueueTexts.Delete(msg);
                                    Log.Information(callPath + " fake hand-off of text (ID=" + msg.Id.ToString() + ") was successfull.");
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

            /*
             * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
             */
            GC.Collect();
#if DEBUG
            Log.Information($"'{callPath}' completed");
#endif
            return Task.CompletedTask;
        }
    }
}
