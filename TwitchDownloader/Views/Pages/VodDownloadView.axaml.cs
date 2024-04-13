using Avalonia.ReactiveUI;
using TwitchDownloader.ViewModels.Pages;

namespace TwitchDownloader.Views.Pages;

public sealed partial class VodDownloadView : ReactiveUserControl<VodDownloadViewModel>
{
    public VodDownloadView()
    {
        InitializeComponent();
    }
}