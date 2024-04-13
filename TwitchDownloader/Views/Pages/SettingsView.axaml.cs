using Avalonia.ReactiveUI;
using TwitchDownloader.ViewModels.Pages;

namespace TwitchDownloader.Views.Pages;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}
