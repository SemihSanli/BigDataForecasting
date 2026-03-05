using BigDataForecasting.API.Dtos.MLDtos;

namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class CustomerWithSalesDto
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public CustomerChurnInput Input { get; set; }
    }
}
