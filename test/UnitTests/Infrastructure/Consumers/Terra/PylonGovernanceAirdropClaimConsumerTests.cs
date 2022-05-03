using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pylonboard.Infrastructure.Consumers.Terra;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack.Data;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using Xunit;

namespace UnitTests.Infrastructure.Consumers.Terra;

public class PylonGovernanceAirdropClaimConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly string _testFilesBasePath;
    private readonly PylonGovernanceAirdropClaimConsumer _consumer;

    public PylonGovernanceAirdropClaimConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new PylonGovernanceAirdropClaimConsumer(
            A.Fake<ILogger<PylonGovernanceAirdropClaimConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public async Task HandlesAirdropClaim()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/pylon_airdrop_claim.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);
        var consumeContext = A.Fake<ConsumeContext<PylonGovernanceAirdropClaimMessage>>();

        // Act
        var actuals = await _consumer.ProcessTransactionAsync(
            terraTx,
            coreTx,
            consumeContext,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(4, actuals.Count());
    }
}