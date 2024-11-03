using System;
using System.Threading;
using Avalonia.Markup.Xaml.MarkupExtensions;
using TwitchDownloader.Models;

namespace TwitchDownloader.Services;

public class LanguageService
{
    public LanguageService()
    {
        DefaultLanguage = Language.English;
    }

    public Language DefaultLanguage { get; }
    public Language CurrentLanguage => Thread.CurrentThread.CurrentUICulture;

    public void SetLanguage(Language language)
    {
        if (string.IsNullOrEmpty(language))
        {
            throw new ArgumentException($"{nameof(language)} can't be empty.");
        }

        I18NExtension.Culture = language;
        Thread.CurrentThread.CurrentUICulture = language;
    }
}
