namespace Pylonboard.ServiceHost.Endpoints.MyRewards;

public record MyRewardsGraph
{
    public IEnumerable<MyRewardsGraphItem> Rewards { get; set; }
}