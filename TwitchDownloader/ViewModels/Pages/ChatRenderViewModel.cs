using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class ChatRenderViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 5;
    public override string PageName => Localizer.Get(ResXLocalizerKeys.ChatRender);

    public override MaterialIconKind PageIconKind => MaterialIconKind.MessageBookmark;
}
