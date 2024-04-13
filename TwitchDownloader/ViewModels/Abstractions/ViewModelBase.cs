using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TwitchDownloader.Helpers;

namespace TwitchDownloader.ViewModels.Abstractions;

[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator, IActivatable
{
    [RelayCommand]
    private void OpenUrl(string url) => EnvironmentHelper.OpenUrl(url);

    public virtual void Activated()
    {
        IsActive = true;
    }

    public virtual void Deactivated()
    {
        IsActive = false;
    }
}
