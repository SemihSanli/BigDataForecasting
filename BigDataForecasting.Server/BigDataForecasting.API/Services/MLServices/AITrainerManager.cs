using BigDataForecasting.API.Dtos.CustomerDtos;
using BigDataForecasting.API.Dtos.GameDtos;
using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Entities;
using BigDataForecasting.API.Repositories;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.BaseServices.GameServices;
using BigDataForecasting.API.Services.BaseServices.SaleServices;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.TimeSeries;

namespace BigDataForecasting.API.Services.MLServices
{
    public class AITrainerManager : IAITrainerService
    {
        private readonly PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> _predictionEnginePool;
        private readonly ICustomerService _customerService;
        private readonly ISaleService _saleService;
        private readonly IGameService _gameService;
        public AITrainerManager(PredictionEnginePool<CustomerChurnInput, CustomerChurnPrediction> predictionEnginePool, ICustomerService customerService, ISaleService saleService, IGameService gameService)
        {
            _predictionEnginePool = predictionEnginePool;
            _customerService = customerService;
            _saleService = saleService;
            _gameService = gameService;
        }

        //public async  Task<List<AdminCLTVResultDto>> GetCLTVPredictionsForAllCustomersAsync()
        //{
        //    var mlContext = new MLContext();
        //    var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "CLTVModel.zip");

        //    if (!File.Exists(modelPath))
        //        throw new FileNotFoundException("CLTV modeli henüz eğitilmemiş! Önce Train işlemini yapınız.");

        //    // 1. Modeli yükle ve Tahmin Motorunu (Engine) oluştur
        //    var model = mlContext.Model.Load(modelPath, out var modelSchema);
        //    var predictionEngine = mlContext.Model.CreatePredictionEngine<CLTVInput, CLTVPrediction>(model);

        //    // 2. Tahmin yapılacak müşterileri getir (Performans için sadece gerekli alanlar)
        //    // Bu metodu daha önce CustomerService içine yazdığını varsayıyorum
        //    var customers = await _customerService.GetActiveUsersAsync();

        //    var results = new List<AdminCLTVResultDto>();

        //    foreach (var customer in customers)
        //    {
        //        // AI'a müşterinin şu anki durumunu veriyoruz
        //        var input = new CLTVInput
        //        {
        //            TotalMoneySpentSoFar = customer.TotalSpent,
        //            TotalGamesBoughtSoFar = customer.TotalGames,
        //            WalletBalance = (float)customer.WalletBalance
        //        };

        //        // YAPAY ZEKA KEHANETİNİ YAPIYOR: "Bu adam bence şu kadar daha harcar"
        //        var prediction = predictionEngine.Predict(input);

        //        // 3. Segmentasyon Mantığı (Business Logic)
        //        string segment = prediction.PredictedFutureValue switch
        //        {
        //            > 2000 => "💎 VIP Müşteri",
        //            > 1000 => "🌟 Sadık Müşteri",
        //            > 500 => "📈 Potansiyeli Yüksek",
        //            _ => "👤 Standart"
        //        };

        //        results.Add(new AdminCLTVResultDto
        //        {
        //            CustomerId = customer.CustomerId,
        //            UserName = customer.UserName,
        //            PredictedFutureValue = (float)Math.Round(prediction.PredictedFutureValue, 2),
        //            CustomerSegment = segment
        //        });
        //    }

        //    // En yüksek değerden başlayarak sırala (Admin'e en değerlileri önce gösterelim)
        //    return results.OrderByDescending(x => x.PredictedFutureValue).ToList();
        //}

        public async Task<List<AdminUserRecommendationResultDto>> GetGameRecommendationsForUserAsync(int customerId, int topN = 5)
        {
            var mlContext = new MLContext();
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Öneri modeli henüz eğitilmemiş! Önce Train işlemi yapınız.");

            // 1. Modeli hızlıca RAM'e yükle
            var model = mlContext.Model.Load(modelPath, out var modelSchema);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<GameRecommendationInput, GameRecommendationPrediction>(model);

            // 2. Kullanıcının zaten SAHİP OLDUĞU oyunları getir (DTO listesi olarak geliyor)
            var ownedGamesDtos = await _saleService.GetOwnedGameIdsByCustomerAsync(customerId);
            var ownedGameIds = ownedGamesDtos.Select(dto => dto.GameId).ToList(); // Hızlı arama için int listesine çevirdik

            // 3. Sistemdeki TÜM oyunların temel bilgilerini getir
            var allGames = await _gameService.GetAllGamesWithBasicDetail();

            // 4. Kuyruk yapısı ile en yüksek skorluları tut
            var recommendations = new PriorityQueue<AdminUserRecommendationResultDto, float>();

            // SADECE adamın SAHİP OLMADIĞI oyunları filtrele ve yapay zekaya sor
            var unownedGames = allGames.Where(g => !ownedGameIds.Contains(g.GameId));

            foreach (var game in unownedGames)
            {
                var input = new GameRecommendationInput
                {
                    CustomerId = (uint)customerId,
                    GameId = (uint)game.GameId
                };

                var prediction = predictionEngine.Predict(input);

                // Eğer AI bu oyunu tavsiye ediyorsa (Skor 0'dan büyükse)
                if (prediction.Score > 0)
                {
                    var resultDto = new AdminUserRecommendationResultDto
                    {
                        GameId = game.GameId,
                        GameName = game.GameName, // İsim servisten geliyor
                        RecommendationScore = prediction.Score
                    };

                    // Kuyruğa ekle (Tersten sıralaması için eksi ile veriyoruz)
                    recommendations.Enqueue(resultDto, -prediction.Score);
                }
            }

            // 5. Kuyruktan en iyi Top N (Örn: 5) tanesini çekip dön
            var topRecommendations = new List<AdminUserRecommendationResultDto>();
            while (recommendations.Count > 0 && topRecommendations.Count < topN)
            {
                topRecommendations.Add(recommendations.Dequeue());
            }

            return topRecommendations;
        }

        public async Task<List<DashboardRandomCustomerRecommendationDto>> GetRandomCustomerRecommendationsAsync()
        {
            // KURAL: Dashboard her zaman 10 rastgele kişi gösterir ve her birine 3 oyun önerir.
            int fixedUserCount = 10;
            int fixedTopN = 3;

            var dashboardResult = new List<DashboardRandomCustomerRecommendationDto>();

            // 1. Sabit 10 rastgele kullanıcıyı çek
            var randomCustomers = await _customerService.GetRandomCustomerAsync(fixedUserCount);
            if (!randomCustomers.Any()) return dashboardResult;

            var customerIds = randomCustomers.Select(c => c.CustomerId).ToList();

            // 2. Performans (Toplu İşlem): 10 müşterinin sahip olduğu oyunları TEK SORGuda al
            var allOwnedGames = await _saleService.GetOwnedGameByMultipleCustomerAsync(customerIds);

            // 3. Tüm oyunları getir
            var allGames = await _gameService.GetAllGamesWithBasicDetail();

            // 4. ML MODELİNİ YÜKLE
            var mlContext = new MLContext();
            var modelPath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");
            if (!File.Exists(modelPath)) return dashboardResult;

            var model = mlContext.Model.Load(modelPath, out var modelSchema);
            var predictionEngine = mlContext.Model.CreatePredictionEngine<GameRecommendationInput, GameRecommendationPrediction>(model);

            // 5. RAM üzerinde hızlı eşleştirme
            foreach (var customer in randomCustomers)
            {
                var customerOwnedGameIds = allOwnedGames
                    .Where(x => x.CustomerId == customer.CustomerId)
                    .Select(x => x.GameId).ToList();

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
                            RecommendationScore = prediction.Score
                        }, -prediction.Score);
                    }
                }

                var topRecommendations = new List<AdminUserRecommendationResultDto>();
                while (recommendations.Count > 0 && topRecommendations.Count < fixedTopN) // Kural: Herkese en iyi 3 oyun
                {
                    topRecommendations.Add(recommendations.Dequeue());
                }

                dashboardResult.Add(new DashboardRandomCustomerRecommendationDto
                {
                    CustomerId = customer.CustomerId,
                    UserName = customer.UserName,
                    RecommendedGames = topRecommendations
                });
            }

            return dashboardResult;
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
                    var prediction = _predictionEnginePool.Predict(modelName: "ChurnModel", example: customerData.Input);
                    var riskPercentage = Math.Round(prediction.Probability * 100, 2);

                    var result = new RiskyCustomerResult
                    {
                        CustomerId = customerData.CustomerId,
                        UserName = customerData.UserName,
                        RiskPercentage = riskPercentage,
                        RawScore = prediction.Score
                    };

                    // DEĞİŞİKLİK 1: Öncelik (priority) olarak artık Score veriyoruz!
                    topRiskyQueue.Enqueue(result, prediction.Score);

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
            return finalResult.OrderByDescending(x => x.RawScore).ToList();
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

        public async Task TrainCLTVModelAsync()
        {
            var trainingData = await _customerService.GetCLTVTrainingDataAsync();

            if (trainingData == null || !trainingData.Any()) return;

            var mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // 2. Regresyon Pipeline'ı (Hangi özelliklere bakıp, neyi tahmin edeceğini söylüyoruz)
            var pipeline = mlContext.Transforms.Concatenate("Features",
                    nameof(CLTVInput.TotalMoneySpentSoFar),
                    nameof(CLTVInput.TotalGamesBoughtSoFar),
                    nameof(CLTVInput.WalletBalance))
                .Append(mlContext.Regression.Trainers.FastTree(
                    labelColumnName: nameof(CLTVInput.FutureSpendingTarget), // Hedefimiz (Gelecekteki Para)
                    featureColumnName: "Features"));

            // 3. Modeli Eğit (Verideki örüntüleri öğrenir)
            var trainedModel = pipeline.Fit(dataView);

            // 4. Modeli Finans Uzmanı olarak kaydet :)
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "CLTVModel.zip");
            mlContext.Model.Save(trainedModel, dataView.Schema, filePath);
        }

        public async Task TrainRecommendationModelAsync()
        {
            var trainingData = await _saleService.GetGameRecommendationDataAsync();

            if (trainingData == null || !trainingData.Any()) return;

            var mlContext = new MLContext(seed: 0);
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingData);

            // Öneri Sistemi Pipeline'ı (Matris Çarpanlarına Ayırma)
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "CustomerIdEncoded", inputColumnName: nameof(GameRecommendationInput.CustomerId))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "GameIdEncoded", inputColumnName: nameof(GameRecommendationInput.GameId)))
                .Append(mlContext.Recommendation().Trainers.MatrixFactorization(new MatrixFactorizationTrainer.Options
                {
                    MatrixColumnIndexColumnName = "CustomerIdEncoded",
                    MatrixRowIndexColumnName = "GameIdEncoded",
                    LabelColumnName = nameof(GameRecommendationInput.Label),
                    NumberOfIterations = 20,
                    ApproximationRank = 100 // Hassasiyet derinliği
                }));

            // Modeli Eğit
            var trainedModel = pipeline.Fit(dataView);

            // Modeli Diske Kaydet
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "GameRecommendationModel.zip");
            mlContext.Model.Save(trainedModel, dataView.Schema, filePath);
        }
    }
}
