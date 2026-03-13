using Microsoft.ML.Data;

namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class CustomerChurnInput
    {
        [LoadColumn(0)] public bool HasChurned { get; set; }
        [LoadColumn(1)] public float TotalMoneySpent { get; set; }
        [LoadColumn(3)] public float TotalGamesBought { get; set; }
        [LoadColumn(6)] public float AverageGamePrice { get; set; } 
    }
}
