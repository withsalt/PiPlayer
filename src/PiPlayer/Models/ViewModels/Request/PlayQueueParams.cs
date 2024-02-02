using System.Collections.Generic;

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
