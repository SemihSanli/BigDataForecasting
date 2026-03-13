using Microsoft.ML.Data;

namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class GameRecommendationInput
    {
        [LoadColumn(0)]
        public uint CustomerId { get; set; } 

        [LoadColumn(1)]
        public uint GameId { get; set; }

        [LoadColumn(2)]
        public float Label { get; set; }
    }
}
