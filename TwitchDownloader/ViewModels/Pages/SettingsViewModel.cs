using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.ViewModels.Pages;

public sealed class SettingsViewModel : PageViewModelBase
{
    public override bool IsFooter => true;
    public override SymbolIconSource Icon { get; } = new() { Symbol = Symbol.Settings};
}