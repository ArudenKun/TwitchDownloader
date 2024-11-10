using System.ComponentModel;
using Avalonia.Controls;
using AvaloniaExtras.Attributes;
using TwitchDownloader.Hosting;

namespace TwitchDownloader;

[StaticViewLocator]
public partial class ViewLocator
{
    public Control Build(object? viewModel)
    {
        if (viewModel is null)
            return new TextBlock { Text = "ViewModel is null" };

        var viewModelType = viewModel.GetType();

        if (!ViewMap.TryGetValue(viewModelType, out var factory))
        {
            return new TextBlock { Text = $"No view registered for {viewModelType.FullName}" };
        }

        var control = factory(viewModel);
        if (viewModel is IActivatable activatable)
        {
            activatable.Bind(control);
        }
        return control;
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}
