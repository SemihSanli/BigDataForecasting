using BigDataForecasting.API.Entities;

namespace BigDataForecasting.API.Dtos.SaleDtos
{
    public class ResultSaleDto
    {
        public int SaleId { get; set; }


        public int CustomerId { get; set; }



        public int GameId { get; set; }




        public DateTime SaleDate { get; set; } = DateTime.Now;


        public decimal SoldPrice { get; set; }

        public string UserName { get; set; }

        public string? Gender { get; set; }


        public string? City { get; set; }


        public string CountryCode { get; set; }


        public long? SteamId { get; set; }
        public string Email { get; set; }

        public string GameName { get; set; }

        public string Description { get; set; }


        public string Genre { get; set; } // ML için önemli (RPG, FPS vs.)
        public double PlayTimeHours { get; set; } = 0;
        public double? Rating { get; set; }
    }
}
