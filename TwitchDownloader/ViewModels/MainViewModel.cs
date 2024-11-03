using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Dialogs;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.ViewModels.Dialogs;
using TwitchDownloader.ViewModels.Pages;

namespace TwitchDownloader.ViewModels;

public sealed partial class MainViewModel : BaseViewModel, ISingletonViewModel
{
    [ObservableProperty]
    private IViewModel _currentPage;

    public MainViewModel(
        VodDownloadViewModel vodDownloadViewModel,
        ClipDownloadViewModel clipDownloadViewModel,
        ChatDownloadViewModel chatDownloadViewModel,
        ChatUpdaterViewModel chatUpdaterViewModel,
        ChatRenderViewModel chatRenderViewModel,
        TaskQueueViewModel taskQueueViewModel,
        SettingsViewModel settingsViewModel
    )
    {
        VodDownloadViewModel = vodDownloadViewModel;
        ClipDownloadViewModel = clipDownloadViewModel;
        ChatDownloadViewModel = chatDownloadViewModel;
        ChatUpdaterViewModel = chatUpdaterViewModel;
        ChatRenderViewModel = chatRenderViewModel;
        TaskQueueViewModel = taskQueueViewModel;
        SettingsViewModel = settingsViewModel;

        CurrentPage = VodDownloadViewModel;
    }

    public VodDownloadViewModel VodDownloadViewModel { get; }
    public ClipDownloadViewModel ClipDownloadViewModel { get; }
    public ChatDownloadViewModel ChatDownloadViewModel { get; }
    public ChatUpdaterViewModel ChatUpdaterViewModel { get; }
    public ChatRenderViewModel ChatRenderViewModel { get; }
    public TaskQueueViewModel TaskQueueViewModel { get; }
    public SettingsViewModel SettingsViewModel { get; }

    [RelayCommand]
    private void OpenSettingsDialog() =>
        DialogManager
            .CreateDialog()
            .WithTitle("Settings")
            .WithContent(SettingsViewModel)
            .WithActionButton("Close", _ => { }, true)
            .TryShow();
}
