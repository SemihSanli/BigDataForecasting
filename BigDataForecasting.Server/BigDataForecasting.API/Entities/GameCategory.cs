namespace BigDataForecasting.API.Entities
{
    public class GameCategory
    {
        public int GameCategoryId { get; set; }
        public string GameCategoryName { get; set; }
        public ICollection<Game> Games { get; set; }
    }
}
