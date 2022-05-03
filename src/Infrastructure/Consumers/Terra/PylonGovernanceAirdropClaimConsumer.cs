using MassTransit;
using Microsoft.Extensions.Logging;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace Pylonboard.Infrastructure.Consumers.Terra;

public class PylonGovernanceAirdropClaimConsumer : IConsumer<PylonGovernanceAirdropClaimMessage>
{
    private readonly ILogger<PylonGovernanceAirdropClaimConsumer> _logger;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IdGenerator _idGenerator;

    public PylonGovernanceAirdropClaimConsumer(
        ILogger<PylonGovernanceAirdropClaimConsumer> logger,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _idGenerator = idGenerator;
    }

    public async Task Consume(ConsumeContext<PylonGovernanceAirdropClaimMessage> context)
    {
        using var db = _dbFactory.OpenDbConnection();
        using var tx = db.OpenTransaction();
        var terraTxId = context.Message.TransactionId;
        _logger.LogInformation("Processing Pylon gov airdrop claim with id: {TxId}", terraTxId);

        var exists = await db.SingleAsync<long?>(db.From<TerraRewardEntity>()
            .Where(q => q.TransactionId == terraTxId)
            .Select(q => q.Id));

        if (exists.HasValue)
        {
            _logger.LogDebug("Rewards for transaction {TransactionId} have already been processed, skipping", terraTxId);
            return;
        }
        var terraDbTx = await db.SingleByIdAsync<TerraRawTransactionEntity>(terraTxId);
        
        var terraTx = terraDbTx.RawTx.ToObject<TerraTxWrapper>();
        var msg = TerraTransactionValueFactory.GetIt(terraTx!);
        var cancellationToken = context.CancellationToken;

        var airdrops = await ProcessTransactionAsync(terraTx!, msg, context, cancellationToken);

        if (airdrops.Any())
        {
            foreach (var airdrop in airdrops)
            {
                await db.InsertAsync(airdrop, token: cancellationToken);
            }
        }
        
        tx.Commit();
    }

    public async Task<List<TerraRewardEntity>> ProcessTransactionAsync(
        TerraTxWrapper tx,
        CoreStdTx msg,
        ConsumeContext<PylonGovernanceAirdropClaimMessage> context,
        CancellationToken cancellationToken
    )
    {
        var airdropClaims = new List<TerraRewardEntity>();
        
        foreach (var properMsg in msg.Messages.Select(innerMsg => innerMsg as WasmMsgExecuteContract))
        {
            if (properMsg == default)
            {
                _logger.LogWarning(
                    "Transaction with id {Id} did not have a message of type {Type}",
                    tx.Id,
                    typeof(WasmMsgExecuteContract)
                );
                continue;
            }
            
            if (properMsg.Value.ExecuteMessage?.Airdrop == null)
            {
                _logger.LogDebug("No airdrop prop, skipping");
                continue;
            }
            
            // AIRDROPS TO CLAAAAIM
            var tokens = msg.Logs
                .QueryTxLogsForAttributes("from_contract", attribute => attribute.Key == "token")
                .ToArray();
            var amounts = msg.Logs
                .QueryTxLogsForAttributes("from_contract", attribute => attribute.Key == "amount")
                .DistinctBy(attribute => attribute.Value)
                .ToArray();

            for (int i = 0; i < tokens.Length; i++)
            {
                var claim = new TerraAmount(amounts[i].Value, tokens[i].Value);

                var reward = new TerraRewardEntity
                {
                    Id = _idGenerator.Snowflake(),
                    Amount = claim.Value,
                    Denominator = TerraDenominators.TryGetDenominator(claim.Denominator),
                    Wallet = properMsg.Value.Sender,
                    CreatedAt = tx.CreatedAt,
                    FromContract = properMsg.Value.Contract,
                    RewardType = TerraRewardType.Airdrop,
                    TransactionId = tx.Id,
                    UpdatedAt = DateTimeOffset.Now,
                    AmountUstNow = null,
                    AmountUstAtClaim = null
                };
                airdropClaims.Add(reward);
            }
        }

        return airdropClaims;
    }
}