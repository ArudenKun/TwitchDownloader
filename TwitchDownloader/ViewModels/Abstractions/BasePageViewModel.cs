using System;
using AvaloniaExtras.Localization;
using Material.Icons;

namespace TwitchDownloader.ViewModels.Abstractions;

public abstract class BasePageViewModel : BaseViewModel, IPageViewModel
{
    protected BasePageViewModel()
    {
        Localizer.LanguageChanged += LocalizerOnLanguageChanged;
    }

    public virtual int PageIndex => 1;
    public virtual string PageName => GetType().Name.Replace("ViewModel", string.Empty);
    public virtual MaterialIconKind PageIconKind => MaterialIconKind.Home;

    private void LocalizerOnLanguageChanged(object? sender, EventArgs e) =>
        OnPropertyChanged(nameof(PageName));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Localizer.LanguageChanged -= LocalizerOnLanguageChanged;
        }

        base.Dispose(disposing);
    }
}
