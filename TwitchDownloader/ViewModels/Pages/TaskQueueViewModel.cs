using AvaloniaExtras.Localization;
using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class TaskQueueViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int PageIndex => 6;
    public override string PageName => Localizer.Get(ResXLocalizerKeys.TaskQueue);
    public override MaterialIconKind PageIconKind => MaterialIconKind.ClipboardList;
}
