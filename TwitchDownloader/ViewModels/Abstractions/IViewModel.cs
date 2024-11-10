using System;
using System.ComponentModel;
using TwitchDownloader.Hosting;

namespace TwitchDownloader.ViewModels.Abstractions;

public interface IViewModel
    : IDisposable,
        IActivatable,
        INotifyPropertyChanged,
        INotifyPropertyChanging
{
    event Action Loaded;
    event Action Unloaded;
}

public interface ISingletonViewModel : IViewModel;

public interface ITransientViewModel : IViewModel;
