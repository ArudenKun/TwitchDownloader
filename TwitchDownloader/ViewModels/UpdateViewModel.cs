using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SukiUI.Controls;
using TwitchDownloader.ViewModels.Abstractions;
using Velopack;
using Velopack.Locators;

namespace TwitchDownloader.ViewModels;

public sealed partial class UpdateViewModel : ViewModelBase
{
    private readonly UpdateManager _updateManager;
    private readonly ILogger<UpdateViewModel> _logger;
    private readonly IVelopackLocator _velopackLocator;

    private UpdateInfo? _updateInfo;

    [ObservableProperty]
    private string _version = "";

    [ObservableProperty]
    private string _currentVersion = "";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    [Range(0, 100)]
    private int _downloadProgress;

    [ObservableProperty]
    [Range(0, 2)]
    private int _stepIndex;

    public IEnumerable<string> Steps { get; } = ["Waiting", "Downloading", "Updating"];

    public UpdateViewModel(UpdateManager updateManager, ILogger<UpdateViewModel> logger)
    {
        _updateManager = updateManager;
        _logger = logger;
        _velopackLocator = VelopackLocator.GetDefault(logger);
    }

    [RelayCommand]
    private async Task Update()
    {
        IsLoading = true;
        if (_updateInfo is null)
        {
            SukiHost.CloseDialog();
            return;
        }

        StepIndex++;
        await _updateManager.DownloadUpdatesAsync(
            _updateInfo,
            progress => DownloadProgress = progress
        );
        StepIndex++;
        await Task.Delay(2000);
        _updateManager.ApplyUpdatesAndRestart(_updateInfo);
    }

    [RelayCommand]
    private void SkipUpdate()
    {
        SukiHost.CloseDialog();
    }

    [RelayCommand]
    private void CloseUpdate()
    {
        SukiHost.CloseDialog();
    }

    public override async void Activated()
    {
        try
        {
            _updateInfo = await _updateManager.CheckForUpdatesAsync();

            if (_updateInfo is null)
            {
                await SukiHost.ShowToast("Updater", "No new version found");
                SukiHost.CloseDialog();
                return;
            }

            CurrentVersion = _velopackLocator.CurrentlyInstalledVersion!.ToFullString();
            Version = _updateInfo.TargetFullRelease.Version.ToFullString();
            _logger.LogInformation("New version found {Version}", Version);
        }
        catch (Exception e)
        {
            await SukiHost.ShowToast("Updater", "An error occured while fetching the update");
            SukiHost.CloseDialog();
            _logger.LogError(e, "Update Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

}
