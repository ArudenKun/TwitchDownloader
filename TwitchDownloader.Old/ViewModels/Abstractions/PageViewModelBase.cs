
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;

namespace TwitchDownloader.ViewModels.Abstractions;

public abstract class PageViewModelBase : ViewModelBase
{
    public virtual string Name => GetName();
    public virtual int Index => 0;

    public virtual SymbolIconSource Icon => new()
    {
        Symbol = Symbol.Home,
    };

    public virtual bool IsFooter => false;
    
    protected virtual string GetName() => GetType().Name.Replace("ViewModel", "");
}