using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SukiUI.Controls;
using TwitchDownloader.SourceGenerators.Attributes;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.Views;
using Velopack;

namespace TwitchDownloader.ViewModels;

[Singleton]
public sealed partial class MainWindowViewModel : ViewModelBase, IActivatable
{
    private readonly UpdateManager _updateManager;
    private readonly ILogger<MainWindowViewModel> _logger;

    public ObservableCollection<PageViewModelBase> Pages { get; }

    public MainWindowViewModel(
        UpdateManager updateManager,
        ILogger<MainWindowViewModel> logger,
        IEnumerable<PageViewModelBase> pages
    )
    {
        _updateManager = updateManager;
        _logger = logger;
        Pages = new ObservableCollection<PageViewModelBase>(pages.OrderBy(x => x.Index));
    }

    public override async void Activated()
    {
        if (!_updateManager.IsInstalled)
        {
            _logger.LogDebug("Velopack is not installed");
            return;
        }

        try
        {
            _logger.LogInformation("Checking for new update");
            var newVersion = await _updateManager.CheckForUpdatesAsync();

            if (newVersion is not null)
            {
                SukiHost.ShowDialog(new UpdateView());
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Fetching Update Error");
        }
    }


    [RelayCommand]
    private void ShowUpdateDialog()
    {
        SukiHost.ShowDialog(new UpdateView());
    }
}
