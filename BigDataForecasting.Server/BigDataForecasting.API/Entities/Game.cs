using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BigDataForecasting.API.Entities
{
    public class Game
    {
    
        public int GameId { get; set; }

       
        public string GameName { get; set; }

        public string Description { get; set; }

    
        public string Genre { get; set; } // ML için önemli (RPG, FPS vs.)

       
        public decimal Price { get; set; }

        public string CoverImageUrl { get; set; }

        // İlişkiler (Bir oyun, binlerce kez satılmış olabilir)
        public  ICollection<Sale> Sales { get; set; }
        public ICollection<GameCategory> GameCategories { get; set; }
    }
}
