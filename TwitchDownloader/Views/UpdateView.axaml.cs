using TwitchDownloader.Views.Abstractions;
using UpdateViewModel = TwitchDownloader.ViewModels.UpdateViewModel;

namespace TwitchDownloader.Views;

public partial class UpdateView : UserControlBase<UpdateViewModel>
{
    public UpdateView()
    {
        InitializeComponent();
    }
}
