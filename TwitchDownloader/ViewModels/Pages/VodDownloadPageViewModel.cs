using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class VodDownloadPageViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int Index => 2;

    // public override string Name => JsonSerializer.Serialize(new { name = LangKeys.VodDownload });
    public override string Name => LangKeys.VodDownload;

    public override MaterialIconKind Icon => MaterialIconKind.Video;
}
