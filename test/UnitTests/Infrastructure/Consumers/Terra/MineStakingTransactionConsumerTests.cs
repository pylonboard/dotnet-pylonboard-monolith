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

public class MineStakingTransactionConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly PylonGovernanceTransactionConsumer _consumer;
    private readonly string _testFilesBasePath;

    public MineStakingTransactionConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new PylonGovernanceTransactionConsumer(
            A.Fake<ILogger<PylonGovernanceTransactionConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public async Task HandleGovStake_works()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/mine_gov_stake.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);
        var consumeContext = A.Fake<ConsumeContext<MineStakingTransactionMessage>>();

        // Act
        var actuals = await _consumer.ProcessTransactionAsync(
            terraTx,
            coreTx,
            consumeContext,
            CancellationToken.None
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(3984.27m, actual.Amount);
        Assert.Equal("terra16hw950c7gznakfe5rcuj4dj8v94em9kdw9wak6", actual.Sender);
        Assert.False(actual.IsBuyBack);
    }

    [Fact]
    public async Task HandleGovUnstake_works()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/mine_gov_unstake.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);
        var consumeContext = A.Fake<ConsumeContext<MineStakingTransactionMessage>>();

        // Act
        var actuals = await _consumer.ProcessTransactionAsync(
            terraTx,
            coreTx,
            consumeContext,
            CancellationToken.None
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(-18000m, actual.Amount);
        Assert.Equal("terra1g0uzl468etgkx0gkts42mg7ly6waqkemw89lsh", actual.Sender);
        Assert.False(actual.IsBuyBack);
    }

    [Fact]
    public async Task IgnoresVotes()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/mine_gov_vote.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);
        var consumeContext = A.Fake<ConsumeContext<MineStakingTransactionMessage>>();

        // Act
        var actuals = await _consumer.ProcessTransactionAsync(
            terraTx,
            coreTx,
            consumeContext,
            CancellationToken.None
        );

        // Assert
        Assert.Empty(actuals);
    }

    [Fact]
    public async Task DispatchesMessageForAirdropClaim()
    {
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/pylon_airdrop_claim.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);
        var consumeContext = A.Fake<ConsumeContext<MineStakingTransactionMessage>>();

        var actuals = await _consumer.ProcessTransactionAsync(
            terraTx,
            coreTx,
            consumeContext,
            CancellationToken.None
        );

        // Assert
        Assert.Empty(actuals);

        A.CallTo(() => consumeContext.Publish(A<PylonGovernanceAirdropClaimMessage>.Ignored, CancellationToken.None))
            .MustHaveHappenedOnceExactly();
    }
}