using BigDataForecasting.API.Dtos.MLDtos;
using BigDataForecasting.API.Services.BaseServices.CustomerServices;
using BigDataForecasting.API.Services.MLServices;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace BigDataForecasting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MLController : ControllerBase
    {
        private readonly IAITrainerService _aiTrainerService;
        private readonly ICustomerService _customerService;

        public MLController(IAITrainerService aiTrainerService, ICustomerService customerService)
        {
            _aiTrainerService = aiTrainerService;
            _customerService = customerService;
        }

        [HttpPost("train-churn-model")]
        public async Task<IActionResult> TrainChurnModel(int pageNumber = 1, int pageSize = 10000)
        {
    
            var customerDataBatch = await _customerService.GetAllCustomerWithSalesAsync(pageNumber, pageSize);

            if (customerDataBatch == null || !customerDataBatch.Any())
            {
                return BadRequest("Eğitim için yeterli veri bulunamadı!");
            }

        
            var trainingData = customerDataBatch.Select(x => x.Input).ToList();

        
            byte[] modelFileBytes = _aiTrainerService.TrainAndSaveModel(trainingData);

      
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "ChurnModel.zip");
            await System.IO.File.WriteAllBytesAsync(filePath, modelFileBytes);

            return Ok(new
            {
                Message = "Model başarıyla eğitildi!",
                RecordCount = trainingData.Count,
                SavedPath = filePath
            });
        }

        //[HttpGet("predict-churn/{customerId}")]
        //public async Task<IActionResult> PredictChurn(int customerId)
        //{
        //    try
        //    {
        //        // 1. Eğitilmiş kahine müşterinin ID'sini verip soruyoruz
        //        var prediction = await _aiTrainerService.PredictionCustomerChurnAsync(customerId);

        //        // 2. Çıkan 0.85 gibi küsuratlı sonucu %85 yapmak için 100 ile çarpıp yuvarlıyoruz
        //        double riskPercentage = Math.Round(prediction.Probability * 100, 2);

        //        // 3. Admin ekranına jilet gibi bir JSON dönüyoruz
        //        return Ok(new 
        //        {
        //            CustomerId = customerId,
        //            WillLeave = prediction.IsChurnedPrediction, // true/false
        //            RiskPercentage = $"%{riskPercentage}",
        //            AI_Message = prediction.IsChurnedPrediction 
        //                ? "DİKKAT: Müşteri bizi terk edebilir! Hemen sistemden indirim maili fırlat." 
        //                : "Sıkıntı yok, bu adam bizden oyun almaya devam eder."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Eğer adamın ID'si DB'de yoksa veya başka hata olursa patlamasın
        //        return BadRequest(new { Error = ex.Message });
        //    }

        //}

        [HttpGet("get-risky-customers-radar")]
        public async Task<IActionResult> GetRiskyCustomersRadar()
        {
            // Hiçbir ID falan vermiyorsun. Direkt metodu çağırıyorsun.
            var riskyCustomers = await _aiTrainerService.GetTopRiskyCustomerAsync();

            if (!riskyCustomers.Any())
            {
                return Ok(new { Message = "Harika! Şu an sistemde bizi terk etme riski olan kimse yok." });
            }

            return Ok(new
            {
                Message = "DİKKAT! Aşağıdaki müşteriler elden gidiyor, hemen kampanya maili at!",
                TotalRiskyCount = riskyCustomers.Count,
                Customers = riskyCustomers
            });
        }
        [HttpGet("predict-revenue")]
        public async Task<IActionResult> PredictFutureRevenue()
        {
            try
            {
                var next3Months = await _aiTrainerService.PredictionNextMonthsRevenueAsync();

                return Ok(new
                {
                    Message = "Gelecek 3 ayın ciro tahmini başarıyla yapıldı.",
                    Month1_Forecast = $"{next3Months[0]:N2} TL",
                    Month2_Forecast = $"{next3Months[1]:N2} TL",
                    Month3_Forecast = $"{next3Months[2]:N2} TL",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        //Hangfire CronJob

        [HttpPost("enqueue-train-job")]
        public IActionResult EnqueueTrainJob()
        {
            // Bu işi kuyruğa at ve hemen 'Ok' dön
            BackgroundJob.Enqueue<IAITrainerService>(x => x.TrainAndSaveModelFromDbAsync());
            return Ok("Eğitim işlemi arka planda başlatıldı. Dashboard'dan takip edebilirsin.");
        }
    }
}