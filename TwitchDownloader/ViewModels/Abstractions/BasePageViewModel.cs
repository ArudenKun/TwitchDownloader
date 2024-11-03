using Material.Icons;

namespace TwitchDownloader.ViewModels.Abstractions;

public abstract partial class BasePageViewModel : BaseViewModel, IPageViewModel
{
    public virtual int Index => 1;

    public virtual string Name => GetType().Name.Replace("PageViewModel", string.Empty);
    public virtual MaterialIconKind Icon => MaterialIconKind.Home;
}
