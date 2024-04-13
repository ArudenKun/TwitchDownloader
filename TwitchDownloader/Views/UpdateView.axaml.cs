using Avalonia.ReactiveUI;
using TwitchDownloader.ViewModels;

namespace TwitchDownloader.Views;

public partial class UpdateView : ReactiveUserControl<UpdateViewModel>
{
    public UpdateView()
    {
        InitializeComponent();
    }
}