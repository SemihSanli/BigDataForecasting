using Microsoft.ML.Data;

namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class CLTVPrediction
    {
        [ColumnName("Score")]
        public float PredictedFutureValue { get; set; }
    }
}
