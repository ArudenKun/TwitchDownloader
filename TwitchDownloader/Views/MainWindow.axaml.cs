using Avalonia.Controls.Primitives;
using TwitchDownloader.ViewModels;
using TwitchDownloader.Views.Abstractions;

namespace TwitchDownloader.Views;

public partial class MainWindow : ReactiveSukiWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        CanResize = false;
    }
}