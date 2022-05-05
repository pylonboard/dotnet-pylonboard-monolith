using MassTransit;
using Microsoft.Extensions.Logging;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using Pylonboard.Kernel;
using Pylonboard.Kernel.Contracts.Exchanges;
using Pylonboard.Kernel.DAL.Entities.Exchanges;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.Infrastructure.Consumers.ExchangeRates;

public class GetExchangeRateConsumer : IConsumer<GetExchangeRate>
{
    private readonly TerraExchangeRateOracle _terraExchangeRateOracle;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IdGenerator _idGenerator;
    private readonly ILogger<GetExchangeRateConsumer> _logger;

    public GetExchangeRateConsumer(
        TerraExchangeRateOracle terraExchangeRateOracle,
        IDbConnectionFactory dbConnectionFactory,
        IdGenerator idGenerator,
        ILogger<GetExchangeRateConsumer> logger
    )
    {
        _terraExchangeRateOracle = terraExchangeRateOracle;
        _dbConnectionFactory = dbConnectionFactory;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GetExchangeRate> context)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync();
        var message = context.Message;
        var rates = await db.SelectAsync(
            db.From<ExchangeMarketCandle>()
                .Where(entity =>
                    entity.Exchange == Exchange.Terra
                    && entity.OpenTime <= message.AtTime.AddMinutes(-15)
                    && message.AtTime <= entity.CloseTime
                    && entity.Market == $"{message.FromDenominator.ToUpperInvariant()}-{message.ToDenominator.ToUpperInvariant()}"
                ).OrderByDescending(q => q.CloseTime)
        );

        if (rates == null || !rates.Any())
        {
            var fetched = await _terraExchangeRateOracle.GetExchangeRateAsync(
                message.FromDenominator,
                message.ToDenominator,
                message.AtTime,
                interval: "15m"
            );
            var candle = new ExchangeMarketCandle
            {
                CloseTime = fetched.closedAt,
                OpenTime = fetched.closedAt.AddMinutes(-15),
                Close = fetched.close,
                Exchange = Exchange.Terra,
                Market = $"{message.FromDenominator.ToUpperInvariant()}-{message.ToDenominator.ToUpperInvariant()}",
                Id = _idGenerator.Snowflake(),
            };
            await db.InsertAsync<ExchangeMarketCandle>(candle, token: context.CancellationToken);

            rates = new List<ExchangeMarketCandle>
            {
                candle
            };
        }

        var theRate = rates[0];
        await context.RespondAsync(new GetExchangeRateResult
        {
            Close = theRate.Close,
            High = theRate.High,
            Low = theRate.Low,
            Open = theRate.Open,
            Volume = theRate.Volume,
            ClosedAt = theRate.CloseTime,
        });
    }
}