using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PiPlayer.Models.Entities;

namespace PiPlayer.Services
{
    public interface IPlayingService : IDisposable
    {
        Task Stop();

        Task<(bool, string)> Play(Media media);

        Task<(bool, string)> PlayQueue(IEnumerable<Media> medium);
    }
}
