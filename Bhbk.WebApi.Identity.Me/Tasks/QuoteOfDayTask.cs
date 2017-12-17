using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.WebApi.Identity.Me.Providers;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Me.Tasks
{
    public class QuoteOfDayTask : CustomIdentityTask
    {
        public readonly QuoteOfDayProvider QuoteOfDay;

        public QuoteOfDayTask()
        {
            QuoteOfDay = new QuoteOfDayProvider();
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(60), cancellationToken);

                    await QuoteOfDay.UpdateAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }
        }
    }
}
