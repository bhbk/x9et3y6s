﻿using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Jobs
{
    [DisallowConcurrentExecution]
    public class MaintainStatesJob : IJob
    {
        private readonly IServiceScopeFactory _factory;

        public MaintainStatesJob(IServiceScopeFactory factory) => _factory = factory;

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
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var invalidExpr = QueryExpressionFactory.GetQueryExpression<tbl_States>()
                            .Where(x => x.ValidFromUtc > DateTime.UtcNow || x.ValidToUtc < DateTime.UtcNow).ToLambda();

                    var invalid = uow.States.Get(invalidExpr);
                    var invalidCount = invalid.Count();

                    if (invalid.Any())
                    {
                        uow.States.Delete(invalid);
                        uow.Commit();

                        var msg = typeof(MaintainStatesJob).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                + invalidCount.ToString() + " invalid states.";

                        Log.Information(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Debug(ex.ToString());
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
