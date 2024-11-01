using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace TwitchDownloader.ViewModels;

[ObservableRecipient]
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILauncher _launcher;

    public MainWindowViewModel(IMessenger messenger, ILauncher launcher)
    {
        Messenger = messenger;
        _launcher = launcher;
    }

#pragma warning disable CA1822 // Mark members as static
    public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static
}