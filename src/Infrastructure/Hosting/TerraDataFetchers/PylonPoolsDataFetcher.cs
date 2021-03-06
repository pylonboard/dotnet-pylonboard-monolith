using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers.Internal.PylonPools;
using Pylonboard.Kernel.DAL.Entities.Terra;
using TerraDotnet;

namespace Pylonboard.Infrastructure.Hosting.TerraDataFetchers;

public class PylonPoolsDataFether
{
    private readonly ILogger<PylonPoolsDataFether> _logger;
    private readonly LowLevelPoolFetcher _poolFetcher;

    public PylonPoolsDataFether(
        ILogger<PylonPoolsDataFether> logger,
        LowLevelPoolFetcher poolFetcher
    )
    {
        _logger = logger;
        _poolFetcher = poolFetcher;
    }

    [Transaction]
    public async Task FetchDataAsync(CancellationToken stoppingToken, bool fullResync = false)
    {
        _logger.LogInformation("Pylon Pool data fetcher commencing");
            
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WHITE_WHALE_1, TerraPylonPoolFriendlyName.WhiteWhale1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WHITE_WHALE_2, TerraPylonPoolFriendlyName.WhiteWhale2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WHITE_WHALE_3, TerraPylonPoolFriendlyName.WhiteWhale3, stoppingToken, fullResync);
            
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.LOOP_1, TerraPylonPoolFriendlyName.Loop1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.LOOP_2, TerraPylonPoolFriendlyName.Loop2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.LOOP_3, TerraPylonPoolFriendlyName.Loop3, stoppingToken, fullResync);

        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.ORION, TerraPylonPoolFriendlyName.Orion, stoppingToken, fullResync);
            
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.VKR_1, TerraPylonPoolFriendlyName.Valkyrie1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.VKR_2, TerraPylonPoolFriendlyName.Valkyrie2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.VKR_3, TerraPylonPoolFriendlyName.Valkyrie3, stoppingToken, fullResync);
            
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.TERRA_WORLD_1, TerraPylonPoolFriendlyName.TerraWorld1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.TERRA_WORLD_2, TerraPylonPoolFriendlyName.TerraWorld2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.TERRA_WORLD_3, TerraPylonPoolFriendlyName.TerraWorld3, stoppingToken, fullResync);
            
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.MINE_1, TerraPylonPoolFriendlyName.Mine1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.MINE_2, TerraPylonPoolFriendlyName.Mine2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.MINE_3, TerraPylonPoolFriendlyName.Mine3, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.NEXUS, TerraPylonPoolFriendlyName.Nexus, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.GLOW_1, TerraPylonPoolFriendlyName.Glow1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.GLOW_2, TerraPylonPoolFriendlyName.Glow2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.GLOW_3, TerraPylonPoolFriendlyName.Glow3, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.SAYVE_1, TerraPylonPoolFriendlyName.Sayve1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.SAYVE_2, TerraPylonPoolFriendlyName.Sayve2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.SAYVE_3, TerraPylonPoolFriendlyName.Sayve3, stoppingToken, fullResync);
        
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.XDEFI_1, TerraPylonPoolFriendlyName.Xdefi1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.XDEFI_2, TerraPylonPoolFriendlyName.Xdefi2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.XDEFI_3, TerraPylonPoolFriendlyName.Xdefi3, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.ARTS_1, TerraPylonPoolFriendlyName.Arts1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.ARTS_2, TerraPylonPoolFriendlyName.Arts2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.ARTS_3, TerraPylonPoolFriendlyName.Arts3, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WCOIN_1, TerraPylonPoolFriendlyName.Wcoin1, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WCOIN_2, TerraPylonPoolFriendlyName.Wcoin2, stoppingToken, fullResync);
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.WCOIN_3, TerraPylonPoolFriendlyName.Wcoin3, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.DEVIANTS_FACTIONS, TerraPylonPoolFriendlyName.DeviantsFactions, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.GALATIC_PUNKS, TerraPylonPoolFriendlyName.GalaticPunks, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.LUNA_BULLS, TerraPylonPoolFriendlyName.LunaBulls, stoppingToken, fullResync);
        
        await _poolFetcher.FetchPoolDataAsync(TerraPylonGatewayContracts.TERRA_BOTS, TerraPylonPoolFriendlyName.TerraBots, stoppingToken, fullResync);
    }
}