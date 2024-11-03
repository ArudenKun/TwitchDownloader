using System;
using System.Reflection;
using Velopack.Locators;

namespace TwitchDownloader.Helpers;

public static class EnvironmentHelper
{
    private static VelopackLocator Locator => VelopackLocator.GetDefault(default);

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

    /// <summary>
    ///     Returns the directory from which the application is run.
    /// </summary>
    public static string AppDirectory => Locator.RootAppDir ?? CurrentDirectory;

    /// <summary>
    ///     Returns the path of the application data.
    /// </summary>
    public static string AppDataDirectory => IsPortable ? PortablePath : RoamingDirectory.JoinPath(AppName);

    /// <summary>
    ///     Returns the path of the portable application data.
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
    /// Indicates whether the current application should save its data in the AppDirectory
    /// </summary>
    public static bool IsPortable => Locator.IsPortable;

    public static bool IsDebug
#if DEBUG
        => true;
#else
        => false;
#endif
}