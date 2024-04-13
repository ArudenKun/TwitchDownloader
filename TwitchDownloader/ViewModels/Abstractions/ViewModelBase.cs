using System.Reactive.Disposables;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ReactiveUI;
using TwitchDownloader.Helpers;

namespace TwitchDownloader.ViewModels.Abstractions;

[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();
    protected ViewModelBase()
    {
        Messenger = WeakReferenceMessenger.Default;
#pragma warning disable IL2026
        IsActive = true;
#pragma warning restore IL2026
        
        this.WhenActivated(disposables =>
        {
            ActivatedAsync().ConfigureAwait(false);
            Activated(disposables);

            Disposable.Create(() =>
            {
                ActivatedAsync().ConfigureAwait(false);
                Deactivated(disposables);
            }).DisposeWith(disposables);
        });
    }

    [RelayCommand]
    private void OpenUrl(string url) => EnvironmentHelper.OpenUrl(url);


    protected virtual void Activated(CompositeDisposable disposables)
    {
    }

    protected virtual void Deactivated(CompositeDisposable disposables)
    {
    }

    protected virtual Task ActivatedAsync()
    {
        return Task.CompletedTask;
    }

    protected virtual Task DeactivatedAsync()
    {
        return Task.CompletedTask;
    }
}