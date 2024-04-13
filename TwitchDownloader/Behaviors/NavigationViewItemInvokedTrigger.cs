using System.Reactive.Disposables;
using Avalonia.Xaml.Interactivity;
using FluentAvalonia.UI.Controls;
using ReactiveMarbles.ObservableEvents;
using TwitchDownloader.Behaviors.Abstractions;
using TwitchDownloader.Extensions;

namespace TwitchDownloader.Behaviors;

public sealed class NavigationViewItemInvokedTrigger : DisposingTrigger<NavigationView>
{
    protected override void OnAttached(CompositeDisposable disposables)
    {
        AssociatedObject.Events().ItemInvoked.Subscribe(OnNext).DisposeWith(disposables);
    }

    private void OnNext(NavigationViewItemInvokedEventArgs obj)
    {
        Interaction.ExecuteActions(AssociatedObject, Actions, obj);
    }
}