using Ardalis.SmartEnum;
using Avalonia.Styling;

namespace TwitchDownloader.Models;

public class Theme : SmartEnum<Theme, string>
{
    public static readonly Theme Default = new("Default", "Default");
    public static readonly Theme Light = new("Light", "Light");
    public static readonly Theme Dark = new("Dark", "Dark");

    public Theme(string name, string value)
        : base(name, value) { }

    public static implicit operator Theme(ThemeVariant themeVariant)
    {
        if (themeVariant == ThemeVariant.Light)
        {
            return Light;
        }

        return themeVariant == ThemeVariant.Dark ? Dark : Default;
    }

    public static implicit operator ThemeVariant(Theme theme) =>
        theme.Name switch
        {
            nameof(Light) => ThemeVariant.Light,
            nameof(Dark) => ThemeVariant.Dark,
            _ => ThemeVariant.Default,
        };
}
