using System.Collections.Generic;

namespace DemoPulse.Models
{
    public class DuelStats
    {
        public ulong KeyA { get; set; }
        public ulong KeyB { get; set; }
        public int WinsA { get; set; }
        public int WinsB { get; set; }
        public int HsA { get; set; }
        public int HsB { get; set; }
        public List<int> TtkMsList { get; } = new();
    }
}
