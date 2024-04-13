using System.Collections.Generic;
using System.Linq;
using AutoInterfaceAttributes;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;

namespace TwitchDownloader.Services;

[AutoInterface]
public sealed class RouterService : IRouterService
{
    private readonly IDialogService _dialogService;
    private const uint HISTORY_MAX_SIZE = 100;
    private ObservableObject _currentViewModel = default!;
    private NavigationViewItem _currentNavigationViewItem = default!;
    private List<NavigationViewItem> _history = [];
    private int _historyIndex = -1;

    public RouterService(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public event Action<ObservableObject>? CurrentViewModelChanged;
    public bool HasNext => _history.Count > 0 && _historyIndex < _history.Count - 1;
    public bool HasPrev => _historyIndex > 0;

    public ObservableObject CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            if (value == _currentViewModel)
                return;
            _currentViewModel = value;
            OnCurrentViewModelChanged(value);
        }
    }

    public NavigationViewItem CurrentNavigationViewItem
    {
        get => _currentNavigationViewItem;
        private set
        {
            if (
                EqualityComparer<NavigationViewItem>
                    .Default
                    .Equals(_currentNavigationViewItem, value)
            )
                return;
            _currentNavigationViewItem = value;
        }
    }

    public void Push(NavigationViewItem item)
    {
        if (HasNext)
            _history = _history.Take(_historyIndex + 1).ToList();

        _history.Add(item);
        _historyIndex = _history.Count - 1;
        if (_history.Count > HISTORY_MAX_SIZE)
            _history.RemoveAt(0);
    }

    public NavigationViewItem? Go(int offset = 0)
    {
        if (offset == 0)
            return default;

        var newIndex = _historyIndex + offset;
        if (newIndex < 0 || newIndex > _history.Count - 1)
            return default;

        _historyIndex = newIndex;
        var navigationViewItem = _history.ElementAt(_historyIndex);
        CurrentViewModel = InstantiateViewModel((Type)navigationViewItem.Tag!);

        return navigationViewItem;
    }

    public void ModifyHistoryIndexByOne()
    {
        _historyIndex -= 1;
    }

    public NavigationViewItem? Back()
    {
        return HasPrev ? Go(-1) : default;
    }

    public NavigationViewItem? Forward()
    {
        return HasNext ? Go(1) : default;
    }

    public void Navigate(NavigationViewItem navigationViewItem)
    {
        if (navigationViewItem.Tag is not Type tag)
        {
            throw new TypeLoadException();
        }

        if (_currentNavigationViewItem == navigationViewItem)
        {
            return;
        }

        var viewModel = InstantiateViewModel(tag);
        CurrentViewModel = viewModel;
        CurrentNavigationViewItem = navigationViewItem;
        Push(navigationViewItem);
    }

    private ObservableObject InstantiateViewModel(Type type) =>
        (ObservableObject)_dialogService.CreateViewModel(type);

    private void OnCurrentViewModelChanged(ObservableObject viewModel)
    {
        CurrentViewModelChanged?.Invoke(viewModel);
    }
}