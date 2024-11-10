using Material.Icons;

namespace TwitchDownloader.ViewModels.Abstractions;

public interface IPageViewModel : IViewModel
{
    int PageIndex { get; }

    string PageName { get; }

    MaterialIconKind PageIconKind { get; }
}
