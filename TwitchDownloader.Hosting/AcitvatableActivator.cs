using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TwitchDownloader.Hosting;

public static class AcitvatableActivator
{
    public static void Bind(this IActivatable? activatable, Control? control)
    {
        if (activatable is null)
        {
            throw new ArgumentNullException(nameof(activatable));
        }

        if (control is null)
        {
            throw new ArgumentNullException(nameof(control));
        }

        control.Loaded += Loaded;
        control.Unloaded += Unloaded;

        return;

        void Loaded(object sender, RoutedEventArgs e)
        {
            activatable.Activate();
        }

        void Unloaded(object sender, RoutedEventArgs e)
        {
            activatable.Deactivate();

            control.Loaded -= Loaded;
            control.Unloaded -= Unloaded;
        }
    }
}