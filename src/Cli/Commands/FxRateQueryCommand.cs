using System.ComponentModel;
using Cli.Infrastructure.Oracles.Fiat;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using ServiceStack;
using Spectre.Console;
using Spectre.Console.Cli;
using TerraDotnet;

namespace Cli.Commands;

internal sealed class FxRateQueryCommand : AsyncCommand<FxRateQueryCommand.Settings>
{
    private readonly TerraExchangeRateOracle _terraOracle;
    private readonly FiatExchangeRateOracle _fiatOracle;

    public FxRateQueryCommand(
        TerraExchangeRateOracle terraOracle,
        FiatExchangeRateOracle fiatOracle
    )
    {
        _terraOracle = terraOracle;
        _fiatOracle = fiatOracle;
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Symbol to query for")]
        [CommandOption("--symbol|-s")]
        public string Symbol { get; init; }

        [Description("Quantity of the given symbol to show the rate / worth of")]
        [CommandOption("--quantity|-q")]
        public decimal Quantity { get; set; } = 1.0m;

        [Description("Reporting currency")]
        [CommandOption("--report|-r")]
        [DefaultValue(TerraDenominators.Ust)]
        public string ReportingSymbol { get; init; }

        [Description("Date and time of the exchange rate to look for")]
        [CommandOption("--at|-a")]
        public DateTimeOffset At { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        return await AnsiConsole.Status()
            .StartAsync("Looking up rates...", async ctx =>
            {
                // Omitted
                var (close, closeAt) = await _terraOracle.GetExchangeRateAsync(
                    settings.Symbol,
                    "UST",
                    settings.At,
                    "1D"
                );


                if (!settings.ReportingSymbol.EqualsIgnoreCase("UST"))
                {
                    close = await _fiatOracle.ConvertToCurrency(
                        settings.At,
                        close,
                        "UST",
                        settings.ReportingSymbol
                    );
                }

                AnsiConsole.MarkupLine(
                    $"[blue]{settings.Quantity:F2} {settings.Symbol}[/] was worth [green]{close*settings.Quantity:F6} {settings.ReportingSymbol}[/] at [teal]{closeAt:D}[/]");

                return 0;
            });
    }
}