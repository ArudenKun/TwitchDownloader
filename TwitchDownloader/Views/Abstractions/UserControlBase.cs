using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.DependencyInjection;
using TwitchDownloader.ViewModels.Abstractions;

namespace TwitchDownloader.Views.Abstractions;

public abstract class UserControlBase<TViewModel> : UserControl where TViewModel : class, INotifyPropertyChanged
{
    public TViewModel ViewModel { get; }

    protected UserControlBase()
    {
        ViewModel = Ioc.Default.GetRequiredService<TViewModel>();
        DataContext = this;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (ViewModel is IActivatable activatable)
        {
            activatable.Activated();
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (ViewModel is IActivatable activatable)
        {
            activatable.Deactivated();
        }
    }
}