﻿using Bhbk.Lib.Identity.Data.EFCore.Infrastructure_DIRECT;
using Bhbk.Lib.Identity.Data.EFCore.Models_DIRECT;
using Bhbk.Lib.QueryExpression.Extensions;
using Bhbk.Lib.QueryExpression.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class MaintainStatesTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay;
        public string Status { get; private set; }

        public MaintainStatesTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            _factory = factory;
            _delay = int.Parse(conf["Tasks:MaintainStates:PollingDelay"]);
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = callPath + " not run yet."
                }, _serializer);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var callPath = $"{MethodBase.GetCurrentMethod().DeclaringType.Name}.{MethodBase.GetCurrentMethod().Name}";

            while (!cancellationToken.IsCancellationRequested)
            {
#if DEBUG
                Log.Information($"'{callPath}' sleeping for {TimeSpan.FromSeconds(_delay)}");
#endif
                await Task.Delay(TimeSpan.FromSeconds(_delay), cancellationToken);
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

                            var msg = typeof(MaintainStatesTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                    + invalidCount.ToString() + " invalid states.";

                            Status = JsonConvert.SerializeObject(
                                new
                                {
                                    status = msg
                                }, _serializer);

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
            }
        }
    }
}
