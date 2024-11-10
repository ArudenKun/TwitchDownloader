using System;
using System.Globalization;
using System.Threading;
using AsyncAwaitBestPractices;
using AvaloniaExtras.Localization;
using TwitchDownloader.Models;

namespace TwitchDownloader.Services;

public class LanguageService
{
    private readonly WeakEventManager<CultureInfo> _languageChangedEventManager = new();

    public Language DefaultLanguage => Language.English;
    public Language CurrentLanguage => Thread.CurrentThread.CurrentUICulture;

    public event Action<CultureInfo>? OnLanguageChanged
    {
        add => _languageChangedEventManager.AddEventHandler(value);
        remove => _languageChangedEventManager.RemoveEventHandler(value);
    }

    public void SetLanguage(Language language)
    {
        if (string.IsNullOrEmpty(language))
        {
            throw new ArgumentException($"{nameof(language)} can't be empty.");
        }

        Thread.CurrentThread.CurrentUICulture = language;
        Localizer.Language = language;
        _languageChangedEventManager.RaiseEvent(language, nameof(OnLanguageChanged));
    }
}
