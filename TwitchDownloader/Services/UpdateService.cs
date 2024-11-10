using Microsoft.Extensions.Logging;
using Velopack;
using Velopack.Sources;

namespace TwitchDownloader.Services;

public class UpdateService
{
    private readonly UpdateManager _updateManager;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _updateManager = new UpdateManager(
            new GithubSource(AppInfo.GITHUB_URL, null, false),
            null,
            logger
        );
    }
}
