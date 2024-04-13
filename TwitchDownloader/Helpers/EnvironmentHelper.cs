using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TwitchDownloader.Helpers;

public static class EnvironmentHelper
{
    private const string PORTABLE_FOLDER = "portable";

    /// <summary>
    /// Returns the version of executing assembly.
    /// </summary>
    public static Version AppVersion =>
        Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

    /// <summary>
    /// Returns the friendly name of this application.
    /// </summary>
    public static string AppFriendlyName => AppDomain.CurrentDomain.FriendlyName;

    /// <summary>
    /// Returns the path of the ApplicationData.
    /// </summary>
    public static string ApplicationDataPath => IsPortable ? GetPortablePath() : GetDefaultPath();

    /// <summary>
    /// Returns the directory from which the application is run.
    /// </summary>
    public static string AppDirectory => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// Indicates whether the current application should save its data in the AppDirectory
    /// </summary>
    public static bool IsPortable => Directory.Exists(GetPortablePath());

    public static bool IsDevBuild => File.Exists(AppDirectory.JoinPath("dev"));

    /// <summary>
    /// Gets or sets the <see cref="T:System.Globalization.CultureInfo" /> object that represents the culture used by the current thread and task-based asynchronous operations.
    /// </summary>
    public static IFormatProvider CurrentCulture => CultureInfo.CurrentCulture;

    /// <summary>
    /// Gets the application data folder path for the user.
    /// </summary>
    /// <param name="parts">Additional path parts to append to the path.</param>
    /// <returns>The application data folder path.</returns>
    public static string GetApplicationDataPath(params string[] parts)
    {
        Directory.CreateDirectory(ApplicationDataPath);
        return ApplicationDataPath.JoinPath(parts);
    }

    public static string GetPortablePath() =>
        PathHelper.GetParent(1, AppDirectory).JoinPath(PORTABLE_FOLDER);

    public static string GetDefaultPath() =>
        Environment
            .GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .JoinPath(AppFriendlyName);

    public static void OpenUrl(Uri uri) => OpenUrl(uri.ToString());
    public static void OpenUrl(string url)
    {
        switch (OSHelper.GetOSVersion())
        {
            case OSVersion.Windows:
                Process.Start(
                    new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true }
                );
                break;
            case OSVersion.Linux:
            default:
                Process.Start("xdg-open", url);
                break;
        }
    }
}
