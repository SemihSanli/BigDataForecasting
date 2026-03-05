namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class CustomerSaleDetailDto
    {
        public int SaleId { get; set; }
        public decimal SoldPrice { get; set; }
        public int GameId { get; set; }
        public DateTime SaleDate { get; set; }
    }
}
