using System.Globalization;
using System.Linq;
using Ardalis.SmartEnum;

namespace TwitchDownloader.Models;

public sealed class Language : SmartEnum<Language, string>
{
    public static readonly Language English = new("en-US", "English");
    public static readonly Language Spanish = new("es-ES", "Español");
    public static readonly Language French = new("fr-FR", "Français");
    public static readonly Language Italian = new("it-it", "Italiano");
    public static readonly Language Japanese = new("ja-JP", "日本語");
    public static readonly Language Polish = new("pl-PL", "Polski");
    public static readonly Language Portuguese = new("pt-BR", "Português (Brasil)");
    public static readonly Language Russian = new("ru-RU", "Русский");
    public static readonly Language Turkish = new("tr-TR", "Türkçe");
    public static readonly Language Ukrainian = new("uk-ua", "Українська");
    public static readonly Language SimplifiedChinese = new("zh-CN", "简体中文（中国大陆）");
    public static readonly Language TraditionalChinese = new("zh-TW", "繁體中文（台灣）");

    public Language(string name, string value)
        : base(name, value) { }

    public static implicit operator Language(CultureInfo culture) =>
        List.FirstOrDefault(x => x.Name == culture.Name)
        ?? new Language(culture.Name, culture.NativeName);

    public static implicit operator CultureInfo(Language language) => new(language.Name);
}
