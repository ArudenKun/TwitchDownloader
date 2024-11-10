using Avalonia.Metadata;

[assembly: XmlnsDefinition(
    "TwitchDownloader.ViewModels",
    "TwitchDownloader.ViewModels.Abstractions"
)]
[assembly: XmlnsDefinition("TwitchDownloader.ViewModels", "TwitchDownloader.ViewModels.Dialogs")]
[assembly: XmlnsDefinition("TwitchDownloader.ViewModels", "TwitchDownloader.ViewModels.Pages")]
[assembly: XmlnsDefinition("TwitchDownloader.ViewModels", "TwitchDownloader.ViewModels")]
[assembly: XmlnsPrefix("TwitchDownloader.ViewModels", "vm")]

[assembly: XmlnsDefinition("TwitchDownloader.Translations", "TwitchDownloader.Translations")]
[assembly: XmlnsPrefix("TwitchDownloader.Translations", "localization")]

// [assembly: XmlnsDefinition("vma", "TwitchDownloader.ViewModels.Abstractions")]
// [assembly: XmlnsDefinition("vmd", "TwitchDownloader.ViewModels.Dialogs")]
// [assembly: XmlnsDefinition("vmp", "TwitchDownloader.ViewModels.Pages")]
// [assembly: XmlnsDefinition("vm", "TwitchDownloader.ViewModels")]

// [assembly: XmlnsPrefix("TwitchDownloader.Views.Dialogs", "vd")]
// [assembly: XmlnsPrefix("TwitchDownloader.Views.Pages", "vp")]
// [assembly: XmlnsPrefix("TwitchDownloader.Views", "v")]
