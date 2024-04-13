using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using TwitchDownloader.SourceGenerators.Attributes;

namespace TwitchDownloader;

[StaticViewLocator]
public sealed partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? viewModel)
    {
        if (viewModel is null)
        {
            return new TextBlock { Text = "The ViewModel passed is null" };
        }

        var type = viewModel.GetType();

        return Registrations.TryGetValue(type, out var func)
            ? func.Invoke()
            : new TextBlock { Text = "Cannot find view for" + type.FullName };
    }

    public bool Match(object? data) => data is INotifyPropertyChanged;
}
