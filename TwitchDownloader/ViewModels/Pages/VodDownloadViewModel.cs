using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using TwitchDownloader.Resources;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public sealed class VodDownloadViewModel : PageViewModelBase
{
    public override string Name => Strings.VodDownload;
    public override SymbolIconSource Icon => new() { Symbol = Symbol.Video };
}