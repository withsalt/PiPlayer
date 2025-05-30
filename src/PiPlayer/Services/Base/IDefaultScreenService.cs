﻿using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PiPlayer.Services.Base
{
    public interface IDefaultScreenService : IDisposable
    {
        Task Show();

        Image<Rgba32> GetDefaultScreenImage();
    }
}
