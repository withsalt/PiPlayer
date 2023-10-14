using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PiPlayer.Models.ViewModels.Request
{
    public class SequenceItem
    {
        public long Id { get; set; }

        public string Type { get; set; }
    }

    public class PlayQueueParams
    {
        public List<SequenceItem> Items { get; set; }
    }
}
