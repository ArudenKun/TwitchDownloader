using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class VodDownloadViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 1;
    public override string PageName => Localizer.Get(ResXLocalizerKeys.VodDownload);
    public override MaterialIconKind PageIconKind => MaterialIconKind.Video;
}
