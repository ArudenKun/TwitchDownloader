using System.Reactive.Disposables;

namespace TwitchDownloader.Extensions;

public static class ObservableExtensions
{
    public static T DisposeWith<T>(this T item, CompositeDisposable compositeDisposable)
        where T : IDisposable
    {
        ArgumentNullException.ThrowIfNull(compositeDisposable);

        compositeDisposable.Add(item);
        return item;
    }
}