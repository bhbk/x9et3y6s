using Bhbk.Lib.DataState.Expressions;
using Bhbk.Lib.Identity.Data.EFCore.Infrastructure;
using Bhbk.Lib.Identity.Data.EFCore.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Bhbk.WebApi.Identity.Admin.Tasks
{
    public class MaintainActivityTask : BackgroundService
    {
        private readonly IServiceScopeFactory _factory;
        private readonly JsonSerializerSettings _serializer;
        private readonly int _delay, _transient, _auditable;
        public string Status { get; private set; }

        public MaintainActivityTask(IServiceScopeFactory factory, IConfiguration conf)
        {
            _factory = factory;
            _delay = int.Parse(conf["Tasks:MaintainActivity:PollingDelay"]);
            _auditable = int.Parse(conf["Tasks:MaintainActivity:HoldAuditable"]);
            _transient = int.Parse(conf["Tasks:MaintainActivity:HoldTransient"]);
            _serializer = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            Status = JsonConvert.SerializeObject(
                new
                {
                    status = typeof(MaintainActivityTask).Name + " not run yet."
                }, _serializer);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_delay), stoppingToken);

                try
                {
                    /*
                     * async database calls from background services should be
                     * avoided so threading issues do not occur.
                     * 
                     * when calling scoped service (unit of work) from a singleton
                     * service (background task) wrap in using block to mimic transient.
                     */

                    using (var scope = _factory.CreateScope())
                    {
                        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        var expiredExpr = new QueryExpression<tbl_Activities>()
                            .Where(x => (x.Created.AddSeconds(_transient) < DateTime.Now && x.Immutable == false)
                                || (x.Created.AddSeconds(_auditable) < DateTime.Now && x.Immutable == true)).ToLambda();

                        var expired = uow.Activities.Get(expiredExpr);
                        var expiredCount = expired.Count();

                        if (expired.Any())
                        {
                            uow.Activities.Delete(expired);
                            uow.Commit();

                            var msg = typeof(MaintainActivityTask).Name + " success on " + DateTime.Now.ToString() + ". Delete "
                                    + expiredCount.ToString() + " expired activity entries.";

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
                    Log.Error(ex.ToString());
                }

                /*
                 * https://docs.microsoft.com/en-us/aspnet/core/performance/memory?view=aspnetcore-3.1
                 */
                GC.Collect();
            }
        }
    }
}
