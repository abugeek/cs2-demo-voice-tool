namespace DemoPulse.Models
{
    public class MatchMetaData
    {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string Map { get; set; } = "";
        public string Server { get; set; } = "";
        public int RoundsCount { get; set; }
        public int ScoreT { get; set; }
        public int ScoreCT { get; set; }
        public int FirstHalfT { get; set; }
        public int FirstHalfCT { get; set; }
        public int SecondHalfT { get; set; }
        public int SecondHalfCT { get; set; }
        public string Winner { get; set; } = "T";
    }
}
