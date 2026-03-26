using BigDataForecasting.API.Dtos.GlobeDtos;

namespace BigDataForecasting.API.Services.BaseServices.GlobeAnalyticsServices
{
    public interface IGlobeAnalyticsService
    {
        Task<GlobeNodeResultDto> GetGlobalNodesAsync();
    }
}
