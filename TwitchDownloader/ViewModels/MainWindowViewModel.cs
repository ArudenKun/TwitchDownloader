using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SukiUI.Dialogs;
using TwitchDownloader.ViewModels.Abstractions;
using TwitchDownloader.ViewModels.Dialogs;

namespace TwitchDownloader.ViewModels;

public sealed partial class MainWindowViewModel : BaseViewModel, ISingletonViewModel
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private IPageViewModel _currentPage;

    public MainWindowViewModel(
        IServiceProvider serviceProvider,
        IEnumerable<IPageViewModel> pageViewModels
    )
    {
        _serviceProvider = serviceProvider;

        Pages = new List<IPageViewModel>(pageViewModels.OrderBy(p => p.Index)).AsReadOnly();
        CurrentPage = Pages[0];
    }

    public IReadOnlyList<IPageViewModel> Pages { get; }

    [RelayCommand]
    private void OpenSettingsDialog() =>
        DialogManager
            .CreateDialog()
            .WithTitle("Settings")
            .WithContent(_serviceProvider.GetRequiredService<SettingsViewModel>())
            .WithActionButton("Close", _ => { }, true)
            .TryShow();
}
