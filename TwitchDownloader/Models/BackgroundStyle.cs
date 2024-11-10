using System;
using System.Linq;
using Ardalis.SmartEnum;
using SukiUI.Enums;

namespace TwitchDownloader.Models;

public class BackgroundStyle : SmartEnum<BackgroundStyle>
{
    public static readonly BackgroundStyle Gradient =
        new(nameof(Gradient), (int)SukiBackgroundStyle.Gradient);

    public static readonly BackgroundStyle Flat = new(nameof(Flat), (int)SukiBackgroundStyle.Flat);

    public static readonly BackgroundStyle Bubble =
        new(nameof(Bubble), (int)SukiBackgroundStyle.Bubble);

    private BackgroundStyle(string name, int value)
        : base(name, value) { }

    public static implicit operator BackgroundStyle(SukiBackgroundStyle style) =>
        List.First(x => x.Name == style.ToString());

    public static implicit operator SukiBackgroundStyle(BackgroundStyle style) =>
        style.Name switch
        {
            nameof(Gradient) => SukiBackgroundStyle.Gradient,
            nameof(Flat) => SukiBackgroundStyle.Flat,
            nameof(Bubble) => SukiBackgroundStyle.Bubble,
            _ => throw new IndexOutOfRangeException("Unknown Background Style"),
        };
}
