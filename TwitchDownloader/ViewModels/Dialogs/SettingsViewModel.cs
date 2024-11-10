using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using AvaloniaExtras.Localization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SukiUI;
using TwitchDownloader.Models;
using TwitchDownloader.Models.Messaging;
using TwitchDownloader.Services;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Dialogs;

public partial class SettingsViewModel : BaseViewModel, ISingletonViewModel
{
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly IStorageProvider _storageProvider;
    private readonly ILauncher _launcher;
    private readonly SettingsService _settingsService;

    public SettingsViewModel(
        ILogger<SettingsViewModel> logger,
        IStorageProvider storageProvider,
        ILauncher launcher,
        SettingsService settingsService
    )
    {
        _logger = logger;
        _storageProvider = storageProvider;
        _launcher = launcher;
        _settingsService = settingsService;

        CacheDirectory = _settingsService.CacheDirectory;
        Language = _settingsService.Language;
        Theme = _settingsService.Theme;
        ThemeColor = _settingsService.ThemeColor;
    }

    #region Cache

    [ObservableProperty]
    private string _cacheDirectory = null!;

    partial void OnCacheDirectoryChanged(string value)
    {
        _settingsService.CacheDirectory = value;
    }

    [RelayCommand]
    private async Task BrowseCacheDirectory()
    {
        var folder = await _storageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { AllowMultiple = false, Title = "Cache Directory" }
        );

        if (folder.Count == 0)
        {
            return;
        }

        CacheDirectory = folder[0].Path.AbsolutePath;
        Language = _settingsService.Language;
    }

    [RelayCommand]
    private async Task OpenCacheDirectory()
    {
        var folder = new DirectoryInfo(CacheDirectory);
        if (!folder.Exists)
        {
            folder.Create();
        }

        await _launcher.LaunchDirectoryInfoAsync(folder);
    }

    #endregion

    #region Language

    [ObservableProperty]
    private Language _language;

    public IReadOnlyCollection<Language> Languages => Language.List;

    partial void OnLanguageChanged(Language? oldValue, Language newValue)
    {
        if (oldValue is null)
        {
            return;
        }

        if (oldValue.Name == newValue.Name)
        {
            return;
        }

        Localizer.Language = newValue;
        _settingsService.Language = newValue;
        _logger.LogInformation("Language changed to: {Language}", newValue.Name);
    }

    #endregion

    #region Theming

    [ObservableProperty]
    private Theme _theme;

    [ObservableProperty]
    private ThemeColor _themeColor;

    [RelayCommand]
    private void ChangeBaseTheme(Theme? theme = null)
    {
        var instance = SukiTheme.GetInstance();

        if (theme is null)
        {
            instance.SwitchBaseTheme();
        }
        else
        {
            instance.ChangeBaseTheme(theme);
        }

        Theme = _settingsService.Theme = instance.ActiveBaseTheme;
        Messenger.Send(new ThemeMessages(Theme));
    }

    #endregion
}
