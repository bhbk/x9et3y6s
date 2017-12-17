using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

//https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/background-tasks-with-ihostedservice

namespace Bhbk.Lib.Identity.Infrastructure
{
    public abstract class CustomIdentityTask : IHostedService
    {
        private Task _executingTask;
        private CancellationTokenSource _cancelationTokenSource;

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            //create a linked token so we can trigger cancellation outside of this token's cancellation
            _cancelationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            //store the task we're executing
            _executingTask = ExecuteAsync(_cancelationTokenSource.Token);

            //if the task is completed then return it, otherwise it's running
            return _executingTask.IsCompleted ? _executingTask : Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            //stop called without start
            if (_executingTask == null)
                return;

            //signal cancellation to the executing method
            _cancelationTokenSource.Cancel();

            //wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(-1, cancellationToken));

            //throw if cancellation triggered
            cancellationToken.ThrowIfCancellationRequested();
        }

        //derived classes should override this and execute a long running method until cancellation is requested
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
