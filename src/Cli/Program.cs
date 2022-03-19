// See https://aka.ms/new-console-template for more information

using Cli.Commands;
using Cli.Infrastructure;
using Cli.Infrastructure.Oracles.Fiat;
using Cli.Infrastructure.Oracles.Fiat.LowLevel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra.LowLevel;
using Refit;
using Serilog;
using Spectre.Console.Cli;
using TerraDotnet.TerraFcd;

var configurationBuilder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/pylonboard-cli.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var services = new ServiceCollection();
services.AddRefitClient<ITerraMoneyFcdApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://fcd.terra.dev"));

services.AddRefitClient<ITerraMoneyExchangeRateApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.coinhall.org"));

services.AddRefitClient<IOpenExchangeRateApiClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://openexchangerates.org"));

services.AddTransient<TerraExchangeRateOracle>();
services.AddTransient<FxRateQueryCommand>();
// To set via ENV variables on nix systems:  export FIATEXCHANGE__ACCESSKEY="YOUR_OPENEXCHANGE_API_KEY"
services.AddOptions<FiatExchangeRateOptions>("FiatExchange");
services.AddTransient<FiatExchangeRateOracle>();

var registrar = new TypeRegistrar(services);
var app = new CommandApp<FxRateQueryCommand>(registrar);

return await app.RunAsync(args);