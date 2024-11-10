using Ardalis.SmartEnum;
using SukiUI.Enums;

namespace TwitchDownloader.Models;

public class ThemeColor : SmartEnum<ThemeColor>
{
    public static readonly ThemeColor Blue = new(nameof(Blue), (int)SukiColor.Blue);
    public static readonly ThemeColor Green = new(nameof(Green), (int)SukiColor.Green);
    public static readonly ThemeColor Red = new(nameof(Red), (int)SukiColor.Red);
    public static readonly ThemeColor Orange = new(nameof(Orange), (int)SukiColor.Orange);

    private ThemeColor(string name, int value)
        : base(name, value) { }
}
