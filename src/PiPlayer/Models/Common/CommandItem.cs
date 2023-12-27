using System.Collections.Generic;
using System.IO;
using CliWrap;
using PiPlayer.Models.Entities;

namespace PiPlayer.Models.Common
{
    public class CommandItem
    {
        public CommandItem(IEnumerable<Media> medium)
        {
            this.Medium = medium;
        }

        public IEnumerable<Media> Medium { get; set; } = new List<Media>();

        public Command Command { get; set; }

        public string PlaylistPath { get; set; }

        /// <summary>
        /// 删除铃声播放文件
        /// </summary>
        public void DeletePlaylist()
        {
            try
            {
                if (string.IsNullOrEmpty(this.PlaylistPath))
                {
                    return;
                }
                if (File.Exists(this.PlaylistPath))
                {
                    File.Delete(this.PlaylistPath);
                }
            }
            catch { }
        }
    }
}
