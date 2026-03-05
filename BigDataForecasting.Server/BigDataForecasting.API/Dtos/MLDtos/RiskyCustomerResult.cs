namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class RiskyCustomerResult
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public double RiskPercentage { get; set; }
        public float RawScore { get; set; }
    }
}
