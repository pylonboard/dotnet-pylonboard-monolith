using System;
using MassTransit;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Pylonboard.ServiceHost.DAL.Migrations;

public class Migration_20220503_139000_ProcessPylonAirdrops : MigrationBase
{
    protected override void ConfigureUpgrade(IMigrationBuilder builder)
    {
        var ctx = ContextAs<PostgreSqlMigrationContext>();
        var connection = ctx.ConnectionProvider.Default();

        builder.Step("queue up pylon airdrops processing", async () =>
        {
            var  txes = connection.SelectLazy<TerraRawTransactionEntity>(@"
select id
from terra_raw_transaction_entity
WHERE jsonb_path_exists(raw_tx,
                        '$.Tx.Value.msg[*].value.contract ? (@ == ""terra1xu8utj38xuw6mjwck4n97enmavlv852zkcvhgp"")')
            and jsonb_path_exists(raw_tx, '$.Tx.Value.msg[*].value.execute_msg.airdrop.claim');
");

            var bus = ctx.Container.Resolve<IBus>();

            foreach (var tx in txes)
            {
                await bus.Publish<PylonGovernanceAirdropClaimMessage>(new PylonGovernanceAirdropClaimMessage
                {
                    TransactionId = tx.Id
                });
            }
        });
    }

    protected override void ConfigureDowngrade(IMigrationBuilder builder)
    {
        throw new System.NotImplementedException();
    }
}