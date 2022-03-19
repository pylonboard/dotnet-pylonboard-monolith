using System.Text.Json;
using Cli.Infrastructure.Oracles.Fiat.LowLevel;
using Microsoft.Extensions.Logging;
using ServiceStack;

namespace Cli.Infrastructure.Oracles.Fiat
{
    public class FiatExchangeRateOracle
    {
        private readonly IOpenExchangeRateApiClient _client;
        private readonly ILogger<FiatExchangeRateOracle> _logger;
        private readonly FiatExchangeRateOptions _options;

        public FiatExchangeRateOracle(
            IOpenExchangeRateApiClient client,
            ILogger<FiatExchangeRateOracle> logger, 
            Microsoft.Extensions.Options.IOptions<FiatExchangeRateOptions> options
        )
        {
            _client = client;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<decimal> ConvertToCurrency(DateTimeOffset at, decimal quantity, string from, string to)
        {
            if (!from.EqualsIgnoreCase("UST") && !from.EqualsIgnoreCase("USD"))
            {
                throw new InvalidOperationException("Must convert from UST or USD");
            }

            var rates = await GetExchangeRatesAsync(at);
            
            var rate = rates.Rates.Single(q => q.Denominator.EqualsIgnoreCase(to));

            return quantity * rate.Rate;
        }
        
        public async Task<FiatApiClientResponse> GetExchangeRatesAsync(DateTimeOffset at)
        {
            var cacheKeyPath = GetCachePath(at);
            var cachedResponse = GetCachedResponse(cacheKeyPath);
            if (cachedResponse is not null)
            {
                return cachedResponse;
            }

            var data = await _client.GetExchangeRatesAsync(
                at.Year.ToString(),
                at.Month.ToString("D2"),
                at.Day.ToString("D2"),
                _options.AccessKey
            );

            if (data.Error != null)
            {
                var error = data.Error;
                _logger.LogCritical(
                    "Error during call to OpenExchangeRates: status {Status} message {Message} - Zen: {Zen}",
                    error.StatusCode, 
                    error.Message, 
                    error.Content
                );
                throw new OperationCanceledException(
                    $"Error during external call. {error.StatusCode} with {error.Message}"
                );
            }
            
            cachedResponse = new FiatApiClientResponse
            {
                At = new DateTimeOffset(at.Year, at.Month, at.Day, 0, 0, 0, TimeSpan.Zero),
                Rates = data.Content.Rates.Select(pair => new FiatExchangeRate
                {
                    Rate = pair.Value,
                    Denominator = pair.Key,
                    Base = data.Content.Base
                })
            };
            await File.WriteAllTextAsync(cacheKeyPath, JsonSerializer.Serialize(cachedResponse));
            
            return cachedResponse;
        }

        private FiatApiClientResponse? GetCachedResponse(string cacheKeyPath)
        {
            if (!File.Exists(cacheKeyPath))
            {
                return default;
            }

            var content = File.ReadAllText(cacheKeyPath);
            return JsonSerializer.Deserialize<FiatApiClientResponse>(content);
        }

        private static string GetCachePath(DateTimeOffset at)
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pylonboard");
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var cacheKey = at.ToUnixTimeSeconds().ToString();
            return Path.Combine(folder, cacheKey);
        }
    }

    public record FiatExchangeRate
    {
        public decimal Rate { get; set; }
        public string Denominator { get; set; }
        
        public string Base { get; set; }
    }

    public record FiatApiClientResponse
    {
        public DateTimeOffset At { get; set; }
        public IEnumerable<FiatExchangeRate> Rates { get; set; }
    }
}