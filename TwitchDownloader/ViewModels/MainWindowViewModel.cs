using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SukiUI.Controls;
using TwitchDownloader.SourceGenerators.Attributes;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.Views;
using Velopack;

namespace TwitchDownloader.ViewModels;

[Singleton]
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly UpdateManager _updateManager;
    private readonly ILogger<MainWindowViewModel> _logger;

    public IEnumerable<PageViewModelBase> Pages { get; }

    public MainWindowViewModel(
        UpdateManager updateManager,
        ILogger<MainWindowViewModel> logger,
        IEnumerable<PageViewModelBase> pages
    )
    {
        _updateManager = updateManager;
        _logger = logger;

        Pages = pages.OrderBy(x => x.Index);
    }

    protected override async Task ActivatedAsync()
    {
        if (!_updateManager.IsInstalled)
        {
            _logger.LogWarning("Velopack is not configured");
            return;
        }

        try
        {
            _logger.LogInformation("Checking for new updates");
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