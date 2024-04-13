using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using TwitchDownloader.Extensions;
using TwitchDownloader.SourceGenerators.Attributes;
using TwitchDownloader.ViewModels.Abstractions;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace TwitchDownloader.ViewModels;

[Singleton]
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly INavigationPageFactory _navigationPageFactory;


    private Frame _frame = null!;

    [ObservableProperty] private NavigationViewItem _currentPage;
    [ObservableProperty] private INotifyPropertyChanged _currentPageContent = null!;

    public MainWindowViewModel(IEnumerable<PageViewModelBase> pages,
        INavigationPageFactory navigationPageFactory)
    {
        _navigationPageFactory = navigationPageFactory;

        pages = pages.ToArray();
        Menus = pages.OrderBy(x => x.Index).Where(x => !x.IsFooter).Select(x => new NavigationViewItem
        {
            Content = x.Name,
            IconSource = x.Icon,
            Tag = x
        }).ToArray();
        Footers = pages.OrderBy(x => x.Index).Where(x => x.IsFooter).Select(x => new NavigationViewItem
        {
            Content = x.Name,
            IconSource = x.Icon,
            Tag = x
        }).ToArray();

        CurrentPage = Menus[0];
    }

    public NavigationViewItem[] Menus { get; }
    public NavigationViewItem[] Footers { get; }

    [RelayCommand]
    private void Loaded(Frame frame)
    {
        _frame = frame;
        _frame.NavigationPageFactory = _navigationPageFactory;
        _frame.NavigateFromObject(Menus[0].Tag);
    }

    [RelayCommand]
    private void Navigate(NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is not NavigationViewItem navigationViewItem)
        {
            return;
        }

        _frame.NavigateFromObject(navigationViewItem.Tag);
    }

    partial void OnCurrentPageChanged(NavigationViewItem? oldValue, NavigationViewItem newValue)
    {
        if (oldValue is not null)
            oldValue.IconSource.As<SymbolIconSource>().IsFilled = false;

        newValue.IconSource.As<SymbolIconSource>().IsFilled = true;
    }
}