using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RainBot.Core;
using RainBot.Core.Models;
using RainBot.ForecastHandler;
using RainBot.Tests.Utils;
using Xunit;

namespace RainBot.Tests.ForecastHandler;
public class ForecastServiceTests
{
    [Fact]
    public async Task ForecastService_ThrowsArgumentNullException_WhenNoForecasts()
    {
        // Arrange
        var forecastRepositoryMock = new Mock<IForecastRepository>();
        var forecastService = new ForecastService(forecastRepositoryMock.Object);

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => forecastService.SyncForecasts(null));
    }

    [Fact]
    public async Task ForecastService_ThrowsArgumentException_WhenForecastsAreEmpty()
    {
        // Arrange
        var forecastRepositoryMock = new Mock<IForecastRepository>();
        var forecastService = new ForecastService(forecastRepositoryMock.Object);

        var forecats = new List<Forecast>();

        // Act/Assert
        await Assert.ThrowsAsync<ArgumentException>(() => forecastService.SyncForecasts(forecats));
    }

    [Theory]
    [MemberData(nameof(GetSampleForecastsAndExpectedResults))]
    public async Task ForecastService_SyncsForecasts_And_ReturnsRainyForecastsWithoutNotifications(
        IReadOnlyList<Forecast> forecastsFromDatabase,
        IReadOnlyList<Forecast> forecastsFromApi,
        IReadOnlyList<Forecast> expectedRainyForecasts,
        IReadOnlyList<Forecast> upsertForecasts)
    {
        // Arrange
        var forecastRepositoryMock = new Mock<IForecastRepository>();
        var forecastService = new ForecastService(forecastRepositoryMock.Object);
        forecastRepositoryMock
            .Setup(fr => fr.GetForecastsByPKAsync(forecastsFromApi))
            .ReturnsAsync(forecastsFromDatabase);
        var forecastComparer = new ForecastComparer();

        // Act
        var rainyForecasts = await forecastService.SyncForecasts(forecastsFromApi);

        // Assert
        Assert.Equal(expectedRainyForecasts, rainyForecasts, forecastComparer);
        forecastRepositoryMock
            .Verify(fr => fr.UpsertForecastsAsync(
                It.Is<IReadOnlyList<Forecast>>(it => it.SequenceEqual(upsertForecasts, forecastComparer))),
            Times.Once);
    }

    public static IEnumerable<object[]> GetSampleForecastsAndExpectedResults()
    {
        return new List<IReadOnlyList<Forecast>[]>
        {
            // There are combination of three types: _ - no forecast, c - cloudy forecast, r - rainy forecast
            // __ in Database | cc from API | __ rainy expected
            GenerateLists("__", "cc", "__"),
            // __ in Database | cr from API | _r rainy expected
            GenerateLists("__", "cr", "_r"),
            // __ in Database | rc from API | r_ rainy expected
            GenerateLists("__", "rc", "r_"),
            // __ in Database | rr from API | rr rainy expected
            GenerateLists("__", "rr", "rr"),

            // c_ in Database | cc from API | __ rainy expected
            GenerateLists("c_", "cc", "__"),
            // c_ in Database | cr from API | _r rainy expected
            GenerateLists("c_", "cr", "_r"),
            // c_ in Database | rc from API | r_ rainy expected
            GenerateLists("c_", "rc", "r_"),
            // c_ in Database | rr from API | rr rainy expected
            GenerateLists("c_", "rr", "rr"),

            // _c in Database | cc from API | __ rainy expected
            GenerateLists("_c", "cc", "__"),
            // _c in Database | cr from API | _r rainy expected
            GenerateLists("_c", "cr", "_r"),
            // _c in Database | rc from API | r_ rainy expected
            GenerateLists("_c", "rc", "r_"),
            // _c in Database | rr from API | rr rainy expected
            GenerateLists("_c", "rr", "rr"),

            // r_ in Database | cc from API | __ rainy expected
            GenerateLists("r_", "cc", "__"),
            // r_ in Database | cr from API | _r rainy expected
            GenerateLists("r_", "cr", "_r"),
            // r_ in Database | rc from API | __ rainy expected
            GenerateLists("r_", "rc", "__"),
            // r_ in Database | rr from API | _r rainy expected
            GenerateLists("r_", "rr", "_r"),

            // _r in Database | cc from API | __ rainy expected
            GenerateLists("_r", "cc", "__"),
            // _r in Database | cr from API | __ rainy expected
            GenerateLists("_r", "cr", "__"),
            // _r in Database | rc from API | r_ rainy expected
            GenerateLists("_r", "rc", "r_"),
            // _r in Database | rr from API | r_ rainy expected
            GenerateLists("_r", "rr", "r_"),

            // cr in Database | cc from API | __ rainy expected
            GenerateLists("cr", "cc", "__"),
            // cr in Database | cr from API | __ rainy expected
            GenerateLists("cr", "cr", "__"),
            // cr in Database | rc from API | r_ rainy expected
            GenerateLists("cr", "rc", "r_"),
            // cr in Database | rr from API | r_ rainy expected
            GenerateLists("cr", "rr", "r_"),

            // rc in Database | cc from API | __ rainy expected
            GenerateLists("rc", "cc", "__"),
            // rc in Database | cr from API | _r rainy expected
            GenerateLists("rc", "cr", "_r"),
            // rc in Database | rc from API | __ rainy expected
            GenerateLists("rc", "rc", "__"),
            // rc in Database | rr from API | _r rainy expected
            GenerateLists("rc", "rr", "_r"),

            // cc in Database | cc from API | __ rainy expected
            GenerateLists("cc", "cc", "__"),
            // cc in Database | cr from API | _r rainy expected
            GenerateLists("cc", "cr", "_r"),
            // cc in Database | rc from API | r_ rainy expected
            GenerateLists("cc", "rc", "r_"),
            // cc in Database | rr from API | rr rainy expected
            GenerateLists("cc", "rr", "rr"),

            // rr in Database | cc from API | __ rainy expected
            GenerateLists("rr", "cc", "__"),
            // rr in Database | cr from API | __ rainy expected
            GenerateLists("rr", "cr", "__"),
            // rr in Database | rc from API | __ rainy expected
            GenerateLists("rr", "rc", "__"),
            // rr in Database | rr from API | __ rainy expected
            GenerateLists("rr", "rr", "__"),
        };
    }

    private static IReadOnlyList<Forecast>[] GenerateLists(string database, string api, string rainy)
    {
        var upsertForecasts = ForecastGenerator.GenerateForecasts(api, true);

        for (int i = 0; i < database.Length; i++)
        {
            if (database[i] == 'r')
            {
                upsertForecasts[i].IsNotified = true;
            }
        }

        return new IReadOnlyList<Forecast>[]
            {
                ForecastGenerator.GenerateForecasts(database, true),
                ForecastGenerator.GenerateForecasts(api, false),
                ForecastGenerator.GenerateForecasts(rainy, true),
                upsertForecasts
            };
    }

    private sealed class ForecastComparer : IEqualityComparer<Forecast>
    {
        public bool Equals(Forecast x, Forecast y) => x.Date == y.Date &&
            x.Condition == y.Condition &&
            x.DayTime == y.DayTime &&
            x.IsNotified == y.IsNotified &&
            x.PrecipitationProbability == y.PrecipitationProbability &&
            x.PrecipitationPeriod == y.PrecipitationPeriod &&
            x.UpdatedAt == y.UpdatedAt;

        public int GetHashCode([DisallowNull] Forecast obj) => obj.GetHashCode();
    }
}
