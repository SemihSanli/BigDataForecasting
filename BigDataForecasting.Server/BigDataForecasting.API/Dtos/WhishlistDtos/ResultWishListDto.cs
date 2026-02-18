namespace BigDataForecasting.API.Dtos.WhishlistDtos
{
    public class ResultWishListDto
    {
        public int WishlistId { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; }
        public string CoverImageUrl { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
