using MassTransit.Initializers;
using Pylonboard.Kernel.DAL.Entities.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MyRewards;

public class MyRewardsService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<MyRewardsService> _logger;

    public MyRewardsService(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<MyRewardsService> logger
    )
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<MyRewardsGraph> GetRewardsAsync(
        string terraWallet,
        TerraRewardType rewardType,
        CancellationToken cancellationToken
    )
    {
        var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var summed = await db.SelectAsync<(string denom, decimal? amount, decimal? amountInUst)>(db
            .From<TerraRewardEntity>()
            .Where(q => q.Wallet == terraWallet && q.RewardType == rewardType)
            .GroupBy(q => q.Denominator)
            .Select(q => new
            {
                q.Denominator,
                Amount = Sql.Sum(q.Amount),
                AmountInUstAtClaim = Sql.Sum(q.AmountUstAtClaim),
            }), token: cancellationToken);
        
        return new MyRewardsGraph
        {
            Rewards = summed.Select(t => new MyRewardsGraphItem
            {
                RewardType = rewardType,
                Denominator = t.denom,
                Amount = t.amount,
                AmountInUst = t.amountInUst,
            })
        };
    }
}