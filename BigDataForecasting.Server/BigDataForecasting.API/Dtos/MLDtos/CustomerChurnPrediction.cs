using Microsoft.ML.Data;

namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class CustomerChurnPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsChurnedPrediction { get; set; }

        public float Probability { get; set; }

        public float Score { get; set; }
    }
}
