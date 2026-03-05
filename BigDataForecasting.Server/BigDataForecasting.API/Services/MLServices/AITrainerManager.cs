using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.BaseServices.SaleServices;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace BigDataForecasting.API.Services.MLServices
{
    public class AITrainerManager : IAITrainerService
    {
        private readonly PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> _predictionEnginePool;
        private readonly ICustomerService _customerService;
        private readonly ISaleService _saleService;
        public AITrainerManager(PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> predictionEnginePool, ICustomerService customerService, ISaleService saleService)
        {
            _predictionEnginePool = predictionEnginePool;
            _customerService = customerService;
            _saleService = saleService;
        }

        public async Task<List<RiskyCustomerResult>> GetTopRiskyCustomerAsync()
        {
            // En yüksek riskli 20 kişiyi tutmak için öncelikli kuyruğumuz.
            var topRiskyQueue = new PriorityQueue<RiskyCustomerResult, double>();
            int maxTopCount = 20;

            int pageNumber = 1;
            int pageSize = 5000;
            bool hasMoreData = true;

            while (hasMoreData)
            {
                // Sadece 3 ana kolonun dolu geldiğinden emin olduğumuz servis çağrısı.
                var pagedCustomers = await _customerService.GetAllCustomerWithSalesAsync(pageNumber, pageSize);

                if (pagedCustomers == null || !pagedCustomers.Any())
                {
                    hasMoreData = false;
                    break;
                }

                foreach (var customerData in pagedCustomers)
                {
                    // Yeni sadeleştirilmiş modelimizle tahmini alıyoruz.
                    var prediction = _predictionEnginePool.Predict(modelName: "ChurnModel", example: customerData.Input);

                    // Olasılığı yüzdeye çeviriyoruz.
                    var riskPercentage = Math.Round(prediction.Probability * 100, 2);

                    var result = new RiskyCustomerResult
                    {
                        CustomerId = customerData.CustomerId,
                        UserName = customerData.UserName,
                        RiskPercentage = riskPercentage,
                        RawScore = prediction.Score // Ham puanı buraya ekledik!
                    };

                    // HİÇBİR ŞART KOYMADAN KUYRUĞA EKLİYORUZ.
                    // Böylece risk %0 olsa bile AI'nın ne düşündüğünü (RawScore) görebileceğiz.
                    topRiskyQueue.Enqueue(result, riskPercentage);

                    if (topRiskyQueue.Count > maxTopCount)
                    {
                        topRiskyQueue.Dequeue();
                    }
                }
                pageNumber++;
            }

            // Kuyruktaki verileri listeye aktarıyoruz.
            var finalResult = new List<RiskyCustomerResult>();
            while (topRiskyQueue.Count > 0)
            {
                finalResult.Add(topRiskyQueue.Dequeue());
            }

            // En riskliyi en üstte görecek şekilde sıralayıp dönüyoruz.
            return finalResult.OrderByDescending(x => x.RiskPercentage).ToList();
        }

        //public async Task<CustomerChurnPrediction> PredictionCustomerChurnAsync(int customerId)
        //{
        //    var customerDto = await _customerService.GetCustomerWithSalesByIdDto(customerId);

        //    if (customerDto == null) throw new Exception("Müşteri sistemde hiç yok!");

        //    // 2. DTO'daki verileri Yapay Zeka modeline (Input) çeviriyoruz
        //    var input = new CustomerChurnInput
        //    {
        //        // Satış listesi boşsa (Count = 0) hata vermez, direkt 0 hesaplar.
        //        TotalMoneySpent = (float)customerDto.Sales.Sum(s => s.SoldPrice),

        //        DaysSinceLastLogin = customerDto.LastLoginDate.HasValue
        //            ? (DateTime.Now - customerDto.LastLoginDate.Value).Days
        //            : 720,

        //        TotalGamesBought = customerDto.Sales.Count
        //    };

        //    // 3. Tahmini Yap
        //    var prediction = _predictionEnginePool.Predict(modelName: "ChurnModel", example: input);

        //    return prediction;
        //}

        public async Task<List<float>> PredictionNextMonthsRevenueAsync()
        {
            var historicalSales = await _saleService.GetMonthlySalesAsync();

            if (historicalSales.Count < 12)
            {
                throw new InvalidOperationException("Yapay zekanın mevsimselliği öğrenebilmesi için en az 12 aylık satış geçmişine ihtiyacı var!");
            }

            var mlData = historicalSales.Select(x => new MonthlyRevenueData
            {
                Revenue = (float)x.TotalRevenue
            }).ToList();

            var mlContext = new MLContext(seed: 0);
            var dataView = mlContext.Data.LoadFromEnumerable(mlData);

            var pipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(RevenueForecastPrediction.ForecastedRevenues),
                inputColumnName: nameof(MonthlyRevenueData.Revenue),
                windowSize: 12,
                seriesLength: mlData.Count,
                trainSize: mlData.Count,
                horizon: 3,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBound",
                confidenceUpperBoundColumn: "UpperBound");

            var model = pipeline.Fit(dataView);
            var forecastingEngine = model.CreateTimeSeriesEngine<MonthlyRevenueData, RevenueForecastPrediction>(mlContext);
            var forecast = forecastingEngine.Predict();

            return forecast.ForecastedRevenues.ToList();
        }

        public byte[] TrainAndSaveModel(List<CustomerChurnInput> trainingData)
        {
            foreach (var item in trainingData)
            {
              
                item.HasChurned = item.DaysSinceLastLogin > 30;
            }
            var mlContext = new MLContext(seed: 0);

            
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            
            var pipeline = mlContext.Transforms.Concatenate("Features",
                nameof(CustomerChurnInput.TotalMoneySpent),
                nameof(CustomerChurnInput.DaysSinceLastLogin),
                nameof(CustomerChurnInput.TotalGamesBought))
            .Append(mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(CustomerChurnInput.HasChurned),
                featureColumnName: "Features"));

           
            var trainedModel = pipeline.Fit(dataView);
           

            using var stream = new MemoryStream();
            mlContext.Model.Save(trainedModel, dataView.Schema, stream);

            return stream.ToArray();
        }

        public async Task TrainAndSaveModelFromDbAsync()
        {
            var allTrainingData = new List<CustomerChurnInput>();
            int pageNumber = 1;
            int pageSize = 10000;
            bool hasMoreData = true;


            while (hasMoreData)
            {
                var batch = await _customerService.GetAllCustomerWithSalesAsync(pageNumber, pageSize);

                if (batch == null || !batch.Any())
                {
                    hasMoreData = false; // Veri bitti, döngüden çık
                }
                else
                {
                    // Gelen paketteki Input'ları ana listeye ekle
                    allTrainingData.AddRange(batch.Select(x => x.Input));
                    pageNumber++;
                }
            }


            if (allTrainingData.Count == 0) return;


            var modelBytes = TrainAndSaveModel(allTrainingData);


            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ChurnModel.zip");
            await File.WriteAllBytesAsync(filePath, modelBytes);
        }
    }
}
