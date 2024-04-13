using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using SukiUI;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public sealed partial class SettingsViewModel : PageViewModelBase
{
    public override int Index => 99;
    public override MaterialIconKind Icon => MaterialIconKind.Settings;

    [RelayCommand]
    private void ChangeTheme()
    {
        SukiTheme.GetInstance().SwitchBaseTheme();
    }
}
