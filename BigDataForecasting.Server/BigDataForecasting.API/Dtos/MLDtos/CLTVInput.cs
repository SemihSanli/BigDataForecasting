using Microsoft.ML.Data;

namespace BigDataForecasting.API.Dtos.MLDtos
{
    public class CLTVInput
    {
        [LoadColumn(0)] public float TotalMoneySpentSoFar { get; set; } // Şu ana kadar harcadığı para
        [LoadColumn(1)] public float TotalGamesBoughtSoFar { get; set; } // Şu ana kadar aldığı oyun sayısı
        [LoadColumn(2)] public float AccountAgeInDays { get; set; } // Kaç gündür platforma üye?
        [LoadColumn(3)] public float DaysSinceLastPurchase { get; set; } // Son alışverişinden bu yana kaç gün geçti?
        [LoadColumn(4)] public float WalletBalance { get; set; } // Cüzdanında şu an bekleyen bakiye

        // Yapay Zekanın öğrenip tahmin edeceği asıl rakam
        [LoadColumn(5), ColumnName("Label")]
        public float FutureSpendingTarget { get; set; }
    }
}
