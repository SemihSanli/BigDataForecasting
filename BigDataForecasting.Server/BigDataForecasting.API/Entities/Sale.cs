using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigDataForecasting.API.Entities
{
    public class Sale
    {
    
        public int SaleId { get; set; }

        // --- İLİŞKİ 1: Müşteri ---
        public int CustomerId { get; set; } // Foreign Key (Veritabanındaki Sütun)

      
        public  Customer Customer { get; set; } // Navigation Property 

        // --- İLİŞKİ 2: Oyun ---
        public int GameId { get; set; } // Foreign Key

      
        public  Game Game { get; set; } // Navigation Property

        // --- Satış Detayları ---
        public DateTime SaleDate { get; set; } = DateTime.Now;

        
        public decimal SoldPrice { get; set; }

        // ML Verileri
        public double PlayTimeHours { get; set; } = 0; 
        public double? Rating { get; set; } 
    }
}
