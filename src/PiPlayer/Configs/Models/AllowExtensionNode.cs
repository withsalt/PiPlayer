using System.Collections.Generic;
using PiPlayer.Models.Enums;

namespace PiPlayer.Configs.Models
{
    public class AllowExtensionNode
    {
        public FileType Type { get; set; }

        public List<string> Values { get; set; }
    }
}
