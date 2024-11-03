using Material.Icons;

namespace TwitchDownloader.ViewModels.Abstractions;

public interface IPageViewModel : IViewModel
{
    int Index { get; }

    string Name { get; }

    MaterialIconKind Icon { get; }
}
