using Hangfire;
using Hangfire.Common;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.RecurringJobs;

namespace Pylonboard.ServiceHost;

public class CronjobManager
{
    private readonly IEnabledServiceRolesConfig _rolesConfig;
    private readonly ILogger<CronjobManager> _logger;
    private readonly RecurringJobManager _jobManager;

    public CronjobManager(
        IEnabledServiceRolesConfig rolesConfig,
        ILogger<CronjobManager> logger,
        RecurringJobManager jobManager
    )
    {
        _rolesConfig = rolesConfig;
        _logger = logger;
        _jobManager = jobManager;
    }

    public virtual Task RegisterJobsIfRequiredAsync()
    {
        if (!_rolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not enabled, not registering jobs");
        }
        _jobManager.AddOrUpdate("psi-arbs",
            Job.FromExpression<PsiPoolArbJob>((job => job.DoWorkAsync(CancellationToken.None))),
            "*/5 * * * *"
        );

        _jobManager.AddOrUpdate("terra-money",
            Job.FromExpression<TerraMoneyRefreshJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "33 * * * *"
        );

        _jobManager.AddOrUpdate("terra-money",
            Job.FromExpression<TerraMoneyRefreshJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "03 * * * *"
        );

        _jobManager.AddOrUpdate("materialized-view-refresh",
            Job.FromExpression<MaterializedViewRefresherJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "13 * * * *"
        );

        _jobManager.AddOrUpdate("fx-rate-download",
            Job.FromExpression<FxRateDownloadJob>(job => job.DoWorkAsync(CancellationToken.None)),
            "43 * * * *"
        );


        return Task.CompletedTask;
    }
}