namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class GetAllActiveStatusCustomerDto
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public string? ProfileImageUrl { get; set; }
        public float TotalSpent { get; set; } 
        public int TotalGames { get; set; }   
        public decimal WalletBalance { get; set; } 
    }
}
