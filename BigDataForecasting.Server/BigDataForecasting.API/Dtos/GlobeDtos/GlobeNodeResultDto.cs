namespace BigDataForecasting.API.Dtos.GlobeDtos
{
    public class GlobeNodeResultDto
    {
        public List<GlobeLocationDto> Locations { get; set; } = new();
        public List<GlobeArcDto> Arcs { get; set; } = new();
    }
}
