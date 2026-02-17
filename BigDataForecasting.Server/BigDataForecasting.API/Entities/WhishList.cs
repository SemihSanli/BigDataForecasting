namespace BigDataForecasting.API.Entities
{
    public class WhishList
    {
        public int WishlistId { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}
