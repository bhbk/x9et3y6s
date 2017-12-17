using Bhbk.Lib.Identity.Infrastructure;
using Bhbk.Lib.Identity.Interfaces;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using BaseLib = Bhbk.Lib.Identity;

namespace Bhbk.WebApi.Identity.Sts.Tasks
{
    public class TestingTask : CustomIdentityTask
    {
        private static IIdentityContext _ioc;

        public TestingTask(IIdentityContext ioc)
        {
            if (ioc == null)
                throw new ArgumentNullException();

            _ioc = ioc;
        }

        public async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(60), cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, BaseLib.Statics.MsgSystemExceptionCaught);
                }
            }
        }
    }
}
