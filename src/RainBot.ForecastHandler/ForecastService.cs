using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using RainBot.Core;
using RainBot.Core.Models;

namespace RainBot.ForecastHandler;

public class ForecastService
{
    private readonly IForecastRepository _forecastRepository;

    public ForecastService(IForecastRepository forecastRepository)
    {
        _forecastRepository = forecastRepository;
    }
    public async Task<IReadOnlyList<Forecast>> SyncForecasts(IReadOnlyList<Forecast> forecastsFromApi)
    {
        Guard.IsNotNull(forecastsFromApi);
        Guard.IsNotEmpty(forecastsFromApi);

        var forecastsFromDatabase = await _forecastRepository.GetForecastsByPKAsync(forecastsFromApi);

        var rainyForecasts = new List<Forecast>(2);

        foreach (var forecastFromApi in forecastsFromApi)
        {
            var forecastFromDatabase = forecastsFromDatabase.FirstOrDefault(wr => wr.Date == forecastFromApi.Date && wr.DayTime == forecastFromApi.DayTime);

            if (forecastFromDatabase != null)
            {
                forecastFromApi.IsNotified = forecastFromDatabase.IsNotified;
            }

            if (forecastFromApi.PrecipitationProbability >= 20 && !forecastFromApi.IsNotified)
            {
                forecastFromApi.IsNotified = true;
                rainyForecasts.Add(forecastFromApi);
            }
        }

        await _forecastRepository.UpsertForecastsAsync(forecastsFromApi);

        return rainyForecasts;
    }
}
