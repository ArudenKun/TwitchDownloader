using TwitchDownloader.Helpers;
using TwitchDownloader.Utilities;

namespace TwitchDownloader;

public static class AppInfo
{
    public static readonly string AppName = nameof(TwitchDownloader);
    public static readonly string AppVersion = EnvironmentHelper.AppVersion.ToString(3);

    public static readonly FilePath DataDir = FilePath.Create(
        EnvironmentHelper.AppDataDirectory,
        isDirectory: true
    );

    public static readonly FilePath Settings = FilePath.Create(
        DataDir.Path.JoinPath("settings.json"),
        false
    );

    public static readonly FilePath LogsDir = FilePath.Create(DataDir.Path.JoinPath("logs"), true);

    public static readonly FilePath CachesDir = FilePath.Create(
        DataDir.Path.JoinPath("caches"),
        true
    );
}