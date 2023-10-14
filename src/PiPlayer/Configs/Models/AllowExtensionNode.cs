using PiPlayer.Models.Enums;
using System.Collections.Generic;

namespace PiPlayer.Configs.Models
{
    public class AllowExtensionNode
    {
        public FileType Type { get; set; }

        public List<string> Values { get; set; }
    }
}
