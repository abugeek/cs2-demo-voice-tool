using System.Collections.Generic;

namespace DemoPulse.Models
{
    public class RoundInfo
    {
        public int RoundNum { get; set; }
        public string Winner { get; set; } = "";
        public string WinType { get; set; } = "";
        public string BombSite { get; set; } = "";
        public List<string> Facts { get; set; } = new();
    }
}
