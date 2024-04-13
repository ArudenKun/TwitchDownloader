using CommunityToolkit.Mvvm.ComponentModel;

namespace TwitchDownloader.ViewModels.Abstractions;

[ObservableRecipient]
public abstract partial class ViewModelBase : ObservableValidator;