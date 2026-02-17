namespace BigDataForecasting.API.Entities
{
    public class GameStat
    {
        public int GameStatId { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public double AverageRating { get; set; } = 0;
        public int TotalLibraryAdds { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
