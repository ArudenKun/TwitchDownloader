using Material.Icons;
using TwitchDownloader.Translations;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public class ChatDownloadPageViewModel : BasePageViewModel, ISingletonViewModel
{
    public override int Index => 1;

    // public override string Name => JsonSerializer.Serialize(new { name = LangKeys.ChatDownload });

    public override string Name => LangKeys.ChatDownload;

    public override MaterialIconKind Icon => MaterialIconKind.Comments;
}
