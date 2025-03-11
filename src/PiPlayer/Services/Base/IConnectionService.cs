using System;
using System.Threading.Tasks;

namespace PiPlayer.Services.Base
{
    public interface IConnectionService : IDisposable
    {
        Task<bool> ConnectAsync();

        Task<bool> DisconnectAsync();
    }
}
