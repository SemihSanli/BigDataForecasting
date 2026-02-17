namespace BigDataForecasting.API.Entities
{
    public class GameDetail
    {
        public int GameDetailId { get; set; }

        // 1:1 ilişki - Her oyunun bir detay kaydı
        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime? ReleaseDate { get; set; }
        public string? Developer { get; set; }
        public bool IsMultiplayer { get; set; } = false;
    }
}

