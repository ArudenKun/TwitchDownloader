using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class ChatUpdaterViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 4;
    public override string PageName => Localizer.Get(ResXLocalizerKeys.ChatUpdater);

    public override MaterialIconKind PageIconKind => MaterialIconKind.MessagePlus;
}
