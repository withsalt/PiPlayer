using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace PiPlayer.Services
{
    public class PiPlayerHostService : IHostedService
    {
        private readonly IdleBus<IFreeSql> _ib;

        public PiPlayerHostService(IdleBus<IFreeSql> ib) 
        {
            _ib = ib;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_ib != null)
            {
                _ib.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
