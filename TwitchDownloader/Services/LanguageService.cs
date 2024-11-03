using System;
using System.Threading;
using Avalonia.Markup.Xaml.MarkupExtensions;
using TwitchDownloader.Models;

namespace TwitchDownloader.Services;

public class LanguageService
{
    public Language DefaultLanguage => Language.English;
    public Language CurrentLanguage => Thread.CurrentThread.CurrentUICulture;

    public void SetLanguage(Language language)
    {
        if (string.IsNullOrEmpty(language))
        {
            throw new ArgumentException($"{nameof(language)} can't be empty.");
        }

        Thread.CurrentThread.CurrentUICulture = language;
        I18NExtension.Culture = language;
    }
}
