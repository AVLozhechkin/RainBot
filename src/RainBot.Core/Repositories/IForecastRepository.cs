using System.Collections.Generic;
using System.Threading.Tasks;
using RainBot.Core.Models;

namespace RainBot.Core;

public interface IForecastRepository
{
    Task UpsertForecastsAsync(IReadOnlyList<Forecast> forecasts);
    Task<IReadOnlyList<Forecast>> GetForecastsByPKAsync(IReadOnlyList<Forecast> forecasts);
}
