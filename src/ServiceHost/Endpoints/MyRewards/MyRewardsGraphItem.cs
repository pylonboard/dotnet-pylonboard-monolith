using Pylonboard.Kernel.DAL.Entities.Terra;

namespace Pylonboard.ServiceHost.Endpoints.MyRewards;

public record MyRewardsGraphItem
{
    public TerraRewardType RewardType { get; set; }
    public string Denominator { get; set; }
    public decimal? Amount { get; set; }
    public decimal? AmountInUst { get; set; }
}