using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Material.Icons;
using SukiUI.Dialogs;
using TwitchDownloader.Models;
using TwitchDownloader.Models.Messaging;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.ViewModels.Dialogs;

namespace TwitchDownloader.ViewModels;

public sealed partial class MainViewModel
    : BaseViewModel,
        ISingletonViewModel,
        IRecipient<ThemeMessages>
{
    public MainViewModel(IEnumerable<IPageViewModel> pages, SettingsViewModel settingsViewModel)
    {
        Pages = pages.OrderBy(x => x.PageIndex).ToList().AsReadOnly();
        SettingsViewModel = settingsViewModel;

        Messenger.Register(this);

        CurrentPage = Pages[0];
    }

    // public MainViewModel(
    //     VodDownloadViewModel vodDownloadViewModel,
    //     ClipDownloadViewModel clipDownloadViewModel,
    //     ChatDownloadViewModel chatDownloadViewModel,
    //     ChatUpdaterViewModel chatUpdaterViewModel,
    //     ChatRenderViewModel chatRenderViewModel,
    //     TaskQueueViewModel taskQueueViewModel,
    //     SettingsViewModel settingsViewModel
    // )
    // {
    //     VodDownloadViewModel = vodDownloadViewModel;
    //     ClipDownloadViewModel = clipDownloadViewModel;
    //     ChatDownloadViewModel = chatDownloadViewModel;
    //     ChatUpdaterViewModel = chatUpdaterViewModel;
    //     ChatRenderViewModel = chatRenderViewModel;
    //     TaskQueueViewModel = taskQueueViewModel;
    //     SettingsViewModel = settingsViewModel;
    //
    //     Messenger.Register(this);
    //
    //     CurrentPage = VodDownloadViewModel;
    // }

    #region Navigation

    [ObservableProperty]
    private IViewModel _currentPage;

    public IReadOnlyList<IPageViewModel> Pages { get; }

    // public VodDownloadViewModel VodDownloadViewModel { get; }
    // public ClipDownloadViewModel ClipDownloadViewModel { get; }
    // public ChatDownloadViewModel ChatDownloadViewModel { get; }
    // public ChatUpdaterViewModel ChatUpdaterViewModel { get; }
    // public ChatRenderViewModel ChatRenderViewModel { get; }
    // public TaskQueueViewModel TaskQueueViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    [RelayCommand]
    private void OpenSettingsDialog() =>
        DialogManager
            .CreateDialog()
            .WithTitle("Settings")
            .WithContent(SettingsViewModel)
            .WithActionButton("Close", _ => { }, true)
            .TryShow();

    #endregion

    #region Theme

    public MaterialIconKind ThemeButtonIconKind =>
        SettingsViewModel.Theme.Name switch
        {
            nameof(Theme.Light) => MaterialIconKind.MoonWaningCrescent,
            nameof(Theme.Dark) => MaterialIconKind.WhiteBalanceSunny,
            _ => MaterialIconKind.ThemeLightDark,
        };

    public void Receive(ThemeMessages message)
    {
        OnPropertyChanged(nameof(ThemeButtonIconKind));
    }

    #endregion
}
