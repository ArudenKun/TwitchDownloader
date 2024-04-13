using System.Reactive.Disposables;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using ReactiveMarbles.ObservableEvents;
using TwitchDownloader.Behaviors.Abstractions;
using TwitchDownloader.Extensions;

namespace TwitchDownloader.Behaviors;

public sealed class LoadedTrigger : DisposingTrigger<Control>
{
    protected override void OnAttached(CompositeDisposable disposables)
    {
        AssociatedObject.Events().Loaded.Subscribe(OnNext).DisposeWith(disposables);
    }

    private void OnNext(RoutedEventArgs obj)
    { 
        Interaction.ExecuteActions(AssociatedObject, Actions, obj);
    }
}