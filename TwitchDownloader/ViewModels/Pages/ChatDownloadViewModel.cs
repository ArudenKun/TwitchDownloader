using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class ChatDownloadViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 3;

    public override string PageName => Localizer.Get(ResXLocalizerKeys.ChatDownload);

    public override MaterialIconKind PageIconKind => MaterialIconKind.Message;
}
