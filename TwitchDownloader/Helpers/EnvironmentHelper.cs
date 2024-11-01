using System;
using System.Globalization;
using System.Reflection;
using Velopack.Locators;

namespace TwitchDownloader.Helpers;

public static class EnvironmentHelper
{
    private static VelopackLocator Locator => VelopackLocator.GetDefault(null);

    /// <summary>
    ///     Returns the version of executing assembly.
    /// </summary>
    public static Version AppVersion => Assembly.GetExecutingAssembly().GetName().Version ?? new Version();

    /// <summary>
    ///     Returns the friendly name of this application.
    /// </summary>
    public static string AppName => AppDomain.CurrentDomain.FriendlyName;

    /// <summary>
    ///     Returns the directory from which the application is run.
    /// </summary>
    public static string CurrentDirectory =>
        Locator.AppContentDir ?? AppDomain.CurrentDomain.BaseDirectory;

    public static string AppDirectory => Locator.RootAppDir ?? CurrentDirectory;

    /// <summary>
    ///     Returns the path of the ApplicationData.
    /// </summary>
    public static string AppDataDirectory => IsPortable ? PortablePath : RoamingDirectory.JoinPath(AppName);

    /// <summary>
    ///     Returns the path of the portable ApplicationData.
    /// </summary>
    public static string PortablePath =>
        AppDirectory.JoinPath("portable");

    /// <summary>
    ///     Returns the path of the roaming directory.
    /// </summary>
    public static string RoamingDirectory =>
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    /// <summary>
    ///     Returns the directory of the user directory (ex: C:\Users\[the name of the user])
    /// </summary>
    public static string UserDirectory =>
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    /// <summary>
    ///     Returns the directory of the downloads directory
    /// </summary>
    public static string DownloadsDirectory => UserDirectory.JoinPath("Downloads");

    /// <summary>
    ///     Gets or sets the <see cref="T:System.Globalization.CultureInfo" /> object that represents the culture used by the
    ///     current thread and task-based asynchronous operations.
    /// </summary>
    public static IFormatProvider CurrentCulture => CultureInfo.CurrentCulture;

    /// <summary>
    /// Indicates whether the current application should save its data in the AppDirectory
    /// </summary>
    public static bool IsPortable => Locator.IsPortable;

    public static string PortableText => IsPortable ? "true" : "false";

    public static bool IsDebug
#if DEBUG
        => true;
#else
        => false;
#endif
}