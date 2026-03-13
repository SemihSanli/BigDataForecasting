namespace BigDataForecasting.API.Dtos.CustomerDtos
{
    public class AdminCLTVResultDto
    {
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public float PredictedFutureValue { get; set; } 
        public string CustomerSegment { get; set; }
    }
}
