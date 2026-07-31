using System.Collections.Generic;

namespace DemoPulse.Models
{
    public class ClutchRecord
    {
        public int RoundNum { get; set; }
        public string PlayerName { get; set; } = "";
        public string Team { get; set; } = "";
        public string ClutchType { get; set; } = ""; // "1v1", "1v2", "1v3", "1v4", "1v5"
        public int VsCount { get; set; }
        public string WinType { get; set; } = "";
        public List<string> Opponents { get; set; } = new();
        public string Details { get; set; } = "";
    }
}
