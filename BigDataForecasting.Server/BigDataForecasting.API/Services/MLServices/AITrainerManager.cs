using BigDataForecasting.API.Dtos.CustomerDtos;
using BigDataForecasting.API.Dtos.GameDtos;
using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.BaseServices.GameServices;
using BigDataForecasting.API.Services.BaseServices.SaleServices;
using BigDataForecasting.API.Services.Caching;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.TimeSeries;
using static BigDataForecasting.API.Constants.CacheKeys.CacheKeys;

namespace BigDataForecasting.API.Services.MLServices
{
    public class AITrainerManager : IAITrainerService
    {
        private readonly PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> _predictionEnginePool;
        private readonly ICustomerService _customerService;
        private readonly ISaleService _saleService;
        private readonly IGameService _gameService;
        private readonly IRedisCachingService _redisCachingService;
        public AITrainerManager(PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> predictionEnginePool, ICustomerService customerService, ISaleService saleService, IGameService gameService, IRedisCachingService redisCachingService)
        {
            _predictionEnginePool = predictionEnginePool;
            _customerService = customerService;
            _saleService = saleService;
            _gameService = gameService;
            _redisCachingService = redisCachingService;
        }

        public async Task<List<AdminCLTVResultDto>> GetCLTVPredictionsForAllCustomersAsync()
        {
            return await _redisCachingService.GetOrAddAsync(ML.AllCLTVPredictions, async () =>
            {
                var mlContext = new MLContext();
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "CLTVModel.zip");

                if (!File.Exists(modelPath))
                    throw new FileNotFoundException("CLTV modeli henüz eğitilmemiş! Önce Train işlemini yapınız.");

                var model = mlContext.Model.Load(modelPath, out var modelSchema);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<CLTVInput, CLTVPrediction>(model);
                var customers = await _customerService.GetAllActiveStatusCustomersAsync();
                var results = new List<AdminCLTVResultDto>();

                foreach (var customer in customers)
                {
                    var input = new CLTVInput
                    {
                        TotalMoneySpentSoFar = customer.TotalSpent,
                        TotalGamesBoughtSoFar = customer.TotalGames,
                        WalletBalance = (float)customer.WalletBalance
                    };

                    var prediction = predictionEngine.Predict(input);

                    string segment = prediction.PredictedFutureValue switch
                    {
                        > 2000 => "💎 VIP Müşteri",
                        > 1000 => "🌟 Sadık Müşteri",
                        > 500 => "📈 Potansiyeli Yüksek",
                        _ => "👤 Standart"
                    };

                    results.Add(new AdminCLTVResultDto
                    {
                        CustomerId = customer.CustomerId,
                        UserName = customer.UserName,
                        ProfileImageUrl = customer.ProfileImageUrl,
                        PredictedFutureValue = (float)Math.Round(prediction.PredictedFutureValue, 2),
                        CustomerSegment = segment
                    });
                }
                return results.OrderByDescending(x => x.PredictedFutureValue).ToList();

            }, TimeSpan.FromHours(2)); 
        }

        public async Task<List<AdminUserRecommendationResultDto>> GetGameRecommendationsForUserAsync(int customerId, int topN = 5)
        {
            string cacheKey = ML.GameRecommendations(customerId);

            return await _redisCachingService.GetOrAddAsync(cacheKey, async () =>
            {
                var mlContext = new MLContext();
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");

                if (!File.Exists(modelPath))
                    throw new FileNotFoundException("Öneri modeli henüz eğitilmemiş! Önce Train işlemi yapınız.");

                var model = mlContext.Model.Load(modelPath, out var modelSchema);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<GameRecommendationInput, GameRecommendationPrediction>(model);

                var ownedGamesDtos = await _saleService.GetOwnedGameIdsByCustomerAsync(customerId);
                var ownedGameIds = ownedGamesDtos.Select(dto => dto.GameId).ToList();
                var allGames = await _gameService.GetAllGamesWithBasicDetail();
                var recommendations = new PriorityQueue<AdminUserRecommendationResultDto, float>();
                var unownedGames = allGames.Where(g => !ownedGameIds.Contains(g.GameId));

                foreach (var game in unownedGames)
                {
                    var input = new GameRecommendationInput { CustomerId = (uint)customerId, GameId = (uint)game.GameId };
                    var prediction = predictionEngine.Predict(input);

                    if (prediction.Score > 0)
                    {
                        var resultDto = new AdminUserRecommendationResultDto
                        {
                            GameId = game.GameId,
                            GameName = game.GameName,
                            RecommendationScore = prediction.Score
                        };
                        recommendations.Enqueue(resultDto, -prediction.Score);
                    }
                }

                var topRecommendations = new List<AdminUserRecommendationResultDto>();
                while (recommendations.Count > 0 && topRecommendations.Count < topN)
                {
                    topRecommendations.Add(recommendations.Dequeue());
                }
                return topRecommendations;

            }, TimeSpan.FromHours(3)); //
        }

        public async Task<List<DashboardRandomCustomerRecommendationDto>> GetRandomCustomerRecommendationsAsync()
        {
            int poolSize = 50;      // 1. Redis'te saklanacak büyük havuzun boyutu
            int displayCount = 10;  // 2. Ekranda gösterilecek rastgele kişi sayısı
            int fixedTopN = 3;      // 3. Her kişiye önerilecek maksimum oyun sayısı

            // --- BÖLÜM 1: HAVUZU HESAPLA VE REDİS'E AT (30 Dakikada 1 Kez Çalışır) ---
            var recommendationPool = await _redisCachingService.GetOrAddAsync(ML.RandomRecommendations, async () =>
            {
                var poolResult = new List<DashboardRandomCustomerRecommendationDto>();

                // Havuz boyutu kadar (50) rastgele kullanıcıyı DB'den çek
                var randomCustomers = await _customerService.GetRandomCustomerAsync(poolSize);
                if (!randomCustomers.Any()) return poolResult;

                var customerIds = randomCustomers.Select(c => c.CustomerId).ToList();
                var allOwnedGames = await _saleService.GetOwnedGameByMultipleCustomerAsync(customerIds);
                var allGames = await _gameService.GetAllGamesWithBasicDetail();

                var mlContext = new MLContext();
                var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");
                if (!File.Exists(modelPath)) return poolResult;

                var model = mlContext.Model.Load(modelPath, out var modelSchema);
                var predictionEngine = mlContext.Model.CreatePredictionEngine<GameRecommendationInput, GameRecommendationPrediction>(model);

                // 50 Kişilik havuzdaki herkes için ML tahminlerini yap
                foreach (var customer in randomCustomers)
                {
                    var customerOwnedGameIds = allOwnedGames.Where(x => x.CustomerId == customer.CustomerId).Select(x => x.GameId).ToList();
                    var unownedGames = allGames.Where(g => !customerOwnedGameIds.Contains(g.GameId));
                    var recommendations = new PriorityQueue<AdminUserRecommendationResultDto, float>();

                    foreach (var game in unownedGames)
                    {
                        var input = new GameRecommendationInput { CustomerId = (uint)customer.CustomerId, GameId = (uint)game.GameId };
                        var prediction = predictionEngine.Predict(input);

                        if (prediction.Score > 0)
                        {
                            recommendations.Enqueue(new AdminUserRecommendationResultDto
                            {
                                GameId = game.GameId,
                                GameName = game.GameName,
                                RecommendationScore = prediction.Score,
                                CoverImageUrl = game.CoverImageUrl
                            }, -prediction.Score);
                        }
                    }

                    var topRecommendations = new List<AdminUserRecommendationResultDto>();
                    while (recommendations.Count > 0 && topRecommendations.Count < fixedTopN)
                    {
                        topRecommendations.Add(recommendations.Dequeue());
                    }

                    poolResult.Add(new DashboardRandomCustomerRecommendationDto
                    {
                        CustomerId = customer.CustomerId,
                        UserName = customer.UserName,
                        ProfileImageUrl = customer.ProfileImageUrl,
                        RecommendedGames = topRecommendations
                    });
                }

                // Hesaplanan 50 kişilik listeyi Redis'e veriyoruz
                return poolResult;

            }, TimeSpan.FromMinutes(30));


            // --- BÖLÜM 2: HAVUZDAN RASTGELE SEÇİM YAP (Her İstekte Anlık Çalışır) ---

            // Eğer Redis'ten gelen havuz boşsa direkt boş liste dön
            if (recommendationPool == null || !recommendationPool.Any())
                return new List<DashboardRandomCustomerRecommendationDto>();

            // 50 Kişilik havuzu RAM üzerinde karıştır (Guid.NewGuid) ve 10 tanesini (displayCount) seç
            var randomTenForDashboard = recommendationPool
                .OrderBy(x => Guid.NewGuid())
                .Take(displayCount)
                .ToList();

            return randomTenForDashboard;
        }

        public async Task<GetTopCLTVDto> GetTopCLTVAsync()
        {
            return await _redisCachingService.GetOrAddAsync(Dashboard.TopCltv, async () =>
            {
                var allPredictions = await GetCLTVPredictionsForAllCustomersAsync();

                var summary = new GetTopCLTVDto
                {
                    TotalCustomerCount = allPredictions.Count,
                    VipCount = allPredictions.Count(x => x.CustomerSegment == "💎 VIP Müşteri"),
                    LoyalCount = allPredictions.Count(x => x.CustomerSegment == "🌟 Sadık Müşteri"),
                    PotentialCount = allPredictions.Count(x => x.CustomerSegment == "📈 Potansiyeli Yüksek"),
                    TopVips = allPredictions.Where(x => x.CustomerSegment == "💎 VIP Müşteri").Take(5).ToList(),
                    TopPotentialCustomers = allPredictions.Where(x => x.CustomerSegment == "📈 Potansiyeli Yüksek").Take(5).ToList()
                };
                return summary;

            }, TimeSpan.FromHours(1));
        }

        public async Task<List<RiskyCustomerResult>> GetTopRiskyCustomerAsync()
        {
            return await _redisCachingService.GetOrAddAsync(ML.TopRiskyCustomers, async () =>
            {
                var topRiskyQueue = new PriorityQueue<RiskyCustomerResult, double>();
                int maxTopCount = 20;
                int pageNumber = 1;
                int pageSize = 5000;
                bool hasMoreData = true;

                while (hasMoreData)
                {
                    var pagedCustomers = await _customerService.GetAllCustomerWithSalesAsync(pageNumber, pageSize);
                    if (pagedCustomers == null || !pagedCustomers.Any())
                    {
                        hasMoreData = false;
                        break;
                    }

                    foreach (var customerData in pagedCustomers)
                    {
                        var prediction = _predictionEnginePool.Predict(modelName: "ChurnModel", example: customerData.Input);
                        var riskPercentage = Math.Round(prediction.Probability * 100, 2);

                        var result = new RiskyCustomerResult
                        {
                            CustomerId = customerData.CustomerId,
                            UserName = customerData.UserName,
                            RiskPercentage = riskPercentage,
                            RawScore = prediction.Score
                        };

                        topRiskyQueue.Enqueue(result, prediction.Score);
                        if (topRiskyQueue.Count > maxTopCount) topRiskyQueue.Dequeue();
                    }
                    pageNumber++;
                }

                var finalResult = new List<RiskyCustomerResult>();
                while (topRiskyQueue.Count > 0) finalResult.Add(topRiskyQueue.Dequeue());
                return finalResult.OrderByDescending(x => x.RawScore).ToList();

            }, TimeSpan.FromHours(2));
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
            return await _redisCachingService.GetOrAddAsync(ML.RevenueForecast, async () =>
            {
                var historicalSales = await _saleService.GetMonthlySalesAsync();

                if (historicalSales.Count < 12)
                    throw new InvalidOperationException("Yapay zekanın mevsimselliği öğrenebilmesi için en az 12 aylık satış geçmişine ihtiyacı var!");

                var mlData = historicalSales.Select(x => new MonthlyRevenueData { Revenue = (float)x.TotalRevenue }).ToList();
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

            }, TimeSpan.FromDays(1)); // Aylık
        }

        public byte[] TrainAndSaveModel(List<CustomerChurnInput> trainingData)
        {

            var mlContext = new MLContext(seed: 0);

            
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            
            var pipeline = mlContext.Transforms.Concatenate("Features",
                nameof(CustomerChurnInput.TotalMoneySpent),
                nameof(CustomerChurnInput.TotalGamesBought))
            .Append(mlContext.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(CustomerChurnInput.HasChurned),
                featureColumnName: "Features"))
            .Append(mlContext.BinaryClassification.Calibrators.Platt(
        labelColumnName: nameof(CustomerChurnInput.HasChurned)));

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
                if (batch == null || !batch.Any()) hasMoreData = false;
                else
                {
                    allTrainingData.AddRange(batch.Select(x => x.Input));
                    pageNumber++;
                }
            }

            if (allTrainingData.Count == 0) return;

            var modelBytes = TrainAndSaveModel(allTrainingData);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ChurnModel.zip");
            await File.WriteAllBytesAsync(filePath, modelBytes);

            // ÇOK KRİTİK: Churn modeli güncellendiğine göre, riskli müşteriler listesi eski kaldı. Sil gitsin!
            await _redisCachingService.RemoveAsync(ML.TopRiskyCustomers);
        }

        public async Task TrainCLTVModelAsync()
        {
            var trainingData = await _customerService.GetCLTVTrainingDataAsync();
            if (trainingData == null || !trainingData.Any()) return;

            var mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = mlContext.Transforms.Concatenate("Features",
                    nameof(CLTVInput.TotalMoneySpentSoFar),
                    nameof(CLTVInput.TotalGamesBoughtSoFar),
                    nameof(CLTVInput.WalletBalance))
                .Append(mlContext.Regression.Trainers.FastTree(
                    labelColumnName: nameof(CLTVInput.FutureSpendingTarget),
                    featureColumnName: "Features"));

            var trainedModel = pipeline.Fit(dataView);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "CLTVModel.zip");
            mlContext.Model.Save(trainedModel, dataView.Schema, filePath);

            // ÇOK KRİTİK: CLTV Modeli güncellendi. Eski tahminlerin hiçbir anlamı kalmadı. Temizle!
            await _redisCachingService.RemoveAsync(Dashboard.TopCltv);
            await _redisCachingService.RemoveAsync(ML.AllCLTVPredictions);
        }

        public async Task TrainRecommendationModelAsync()
        {
            var trainingData = await _saleService.GetGameRecommendationDataAsync();
            if (trainingData == null || !trainingData.Any()) return;

            var mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "CustomerIdEncoded", inputColumnName: nameof(GameRecommendationInput.CustomerId))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "GameIdEncoded", inputColumnName: nameof(GameRecommendationInput.GameId)))
                .Append(mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "CustomerIdEncoded",
                    MatrixRowIndexColumnName = "GameIdEncoded",
                    LabelColumnName = nameof(GameRecommendationInput.Label),
                    NumberOfIterations = 20,
                    ApproximationRank = 100
                }));

            var trainedModel = pipeline.Fit(dataView);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");
            mlContext.Model.Save(trainedModel, dataView.Schema, filePath);

            // ÇOK KRİTİK: Öneri mekanizması değişti. Dashboard'daki ve genel listelerdeki önerileri patlat!
            await _redisCachingService.RemoveAsync(ML.RandomRecommendations);
        }
    }
}
