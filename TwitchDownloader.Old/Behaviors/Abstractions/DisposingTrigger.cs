using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Xaml.Interactivity;

namespace TwitchDownloader.Behaviors.Abstractions;

public abstract class DisposingTrigger<T> : Trigger<T>
    where T : AvaloniaObject
{
    private readonly CompositeDisposable _disposables = new();

#pragma warning disable IL2046
    protected override void OnAttached()
#pragma warning restore IL2046
    {
        base.OnAttached();

        OnAttached(_disposables);
    }

    protected abstract void OnAttached(CompositeDisposable disposables);

    protected override void OnDetaching()
    {
        base.OnDetaching();

        _disposables.Dispose();
    }
}