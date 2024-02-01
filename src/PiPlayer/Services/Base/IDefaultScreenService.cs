using System;
using System.Threading.Tasks;

namespace PiPlayer.Services.Base
{
    public interface IDefaultScreenService : IDisposable
    {
        Task Show(bool isRefresh = false);
    }
}
