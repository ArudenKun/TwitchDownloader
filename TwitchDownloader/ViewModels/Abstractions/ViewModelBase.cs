using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using TwitchDownloader.Helpers;

namespace TwitchDownloader.ViewModels.Abstractions;

[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator, IActivatable
{
    [RelayCommand]
    private void OpenUrl(string url) => EnvironmentHelper.OpenUrl(url);

    protected ViewModelBase()
    {
        Messenger = StrongReferenceMessenger.Default;
    }

    [UnconditionalSuppressMessage("Trimming",
        "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
        Justification = "<Pending>")]
    public void Activated()
    {
        Messenger.RegisterAll(this);
        HandleActivated();
    }

    public void Deactivated()
    {
        Messenger.UnregisterAll(this);
        HandleDeactivated();
    }
    
    protected virtual void HandleActivated()
    {
    }

    protected virtual void HandleDeactivated()
    {
    }
}