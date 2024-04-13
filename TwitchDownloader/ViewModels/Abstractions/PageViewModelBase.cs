using Humanizer;
using Material.Icons;

namespace TwitchDownloader.ViewModels.Abstractions;

public abstract class PageViewModelBase : ViewModelBase
{
    public virtual string DisplayName => GetName();
    public virtual int Index => 0;
    public virtual MaterialIconKind Icon => MaterialIconKind.Home;

    protected virtual string GetName() => GetType().Name.Replace("ViewModel", "").Titleize();
}
