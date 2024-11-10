using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class ClipDownloadViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 2;
    public override string PageName => Localizer.Get(ResXLocalizerKeys.ClipDownload);
    public override MaterialIconKind PageIconKind => MaterialIconKind.Clapperboard;
}
