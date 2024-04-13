using TwitchDownloader.ViewModels.Pages;
using TwitchDownloader.Views.Abstractions;

namespace TwitchDownloader.Views.Pages;

public sealed partial class VodDownloadView : UserControlBase<VodDownloadViewModel>
{
    public VodDownloadView()
    {
        InitializeComponent();
    }
}
