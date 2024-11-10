using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using Ardalis.SmartEnum.SystemTextJson;
using Cogwheel;
using Microsoft.Extensions.Logging;
using Serilog.Core;
using Serilog.Events;
using TwitchDownloader.Models;

namespace TwitchDownloader.Services;

public sealed partial class SettingsService : SettingsBase
{
    private readonly ILogger<SettingsService> _logger;
    private readonly LoggingLevelSwitch _loggingLevelSwitch;

    private LogEventLevel _logLevel = LogEventLevel.Information;

    public SettingsService(ILogger<SettingsService> logger, LoggingLevelSwitch loggingLevelSwitch)
        : base(AppInfo.Settings, JsonContext.Default.Options)
    {
        _logger = logger;
        _loggingLevelSwitch = loggingLevelSwitch;

        Load();
    }

    public bool CheckForUpdate { get; set; }

    [JsonConverter(typeof(SmartEnumNameConverter<Language, string>))]
    public Language Language { get; set; } =
        Language.List.FirstOrDefault(x => x.Name == CultureInfo.CurrentCulture.Name)
        ?? Language.English;

    public TimeFormat TimeFormat { get; set; } = TimeFormat.Local;

    public LogEventLevel LogLevel
    {
        get => _logLevel;
        set => _logLevel = _loggingLevelSwitch.MinimumLevel = value;
    }

    public string CacheDirectory { get; set; } = AppInfo.CachesDir;

    [JsonConverter(typeof(SmartEnumNameConverter<Theme, string>))]
    public Theme Theme { get; set; } = Theme.Default;

    [JsonConverter(typeof(SmartEnumNameConverter<ThemeColor, int>))]
    public ThemeColor ThemeColor { get; set; } = ThemeColor.Blue;

    public override void Save()
    {
        base.Save();
        _logger.LogInformation("Saved settings to {Path}", AppInfo.Settings.Path);
    }

    public override bool Load()
    {
        var result = base.Load();
        if (result)
        {
            _logger.LogInformation("Loaded settings from {Path}", AppInfo.Settings.Path);
            return result;
        }

        _logger.LogInformation("Failed to load settings, Loading defaults");
        Reset();
        _logger.LogInformation("Loaded default settings.");
        return result;
    }

    [JsonSerializable(typeof(SettingsService))]
    [JsonSourceGenerationOptions(
#if DEBUG
        WriteIndented = true,
#endif
        UseStringEnumConverter = true,
        PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
        AllowTrailingCommas = true
    )]
    private sealed partial class JsonContext : JsonSerializerContext;
}
