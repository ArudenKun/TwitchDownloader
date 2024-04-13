using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;

namespace TwitchDownloader.Controls;

public sealed class EntranceTransition : IPageTransition
{
    /// <summary>
    ///     Gets or sets the Horizontal Offset used when animating
    /// </summary>
    public double FromHorizontalOffset { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the Vertical Offset used when animating
    /// </summary>
    public double FromVerticalOffset { get; set; } = 100;

    public async Task Start(
        Visual? from,
        Visual? to,
        bool forward,
        CancellationToken cancellationToken
    )
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        if (from is not null)
            from.IsVisible = false;

        var animation = new Animation
        {
            Easing = new SplineEasing(0.1, 0.9, 0.2),
            Children =
            {
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 0.0),
                        new Setter(TranslateTransform.XProperty, FromHorizontalOffset),
                        new Setter(TranslateTransform.YProperty, FromVerticalOffset)
                    },
                    Cue = new Cue(0d)
                },
                new KeyFrame
                {
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, 1d),
                        new Setter(TranslateTransform.XProperty, 0.0),
                        new Setter(TranslateTransform.YProperty, 0.0)
                    },
                    Cue = new Cue(1d)
                }
            },
            Duration = TimeSpan.FromSeconds(0.5),
            FillMode = FillMode.Forward
        };

        if (to != null)
        {
            await animation.RunAsync(to, cancellationToken);
            to.Opacity = 1;
        }
    }
}