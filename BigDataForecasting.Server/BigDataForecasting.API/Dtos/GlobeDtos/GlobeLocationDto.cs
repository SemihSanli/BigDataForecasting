namespace BigDataForecasting.API.Dtos.GlobeDtos
{
    public class GlobeLocationDto
    {
        public string City { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Value { get; set; }
        public int CustomerCount { get; set; }
        public string Color { get; set; } = "#D4AF37";
    }
}
