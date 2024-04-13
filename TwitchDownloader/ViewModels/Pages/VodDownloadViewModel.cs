using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using TwitchDownloader.Resources;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public sealed partial class VodDownloadViewModel : PageViewModelBase
{
    [ObservableProperty]
    private int _randomNumber;

    public override string DisplayName => Strings.VodDownload;
    public override int Index => 1;
    public override MaterialIconKind Icon => MaterialIconKind.Video;

    public override void Activated()
    {
        RandomNumber = Random.Shared.Next(1, 10);
    }

}
