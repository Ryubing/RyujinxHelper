using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Discord;
using Gommon;
using Volte.Entities;
using Color = System.Drawing.Color;

namespace Volte.UI.Converters;

public class LogSeverityToBrush : VolteBrushConverter<LogSeverityToBrush>
{
    public LogSeverityToBrush()
    {
        BrushDefinitions = [
            (LogSeverity.Critical, GetBrush(Color.Maroon)),
            (LogSeverity.Error, GetBrush(Color.DarkRed)),
            (LogSeverity.Warning, GetBrush(Color.Yellow)),
            (LogSeverity.Info, GetBrush(Color.SpringGreen)),
            (LogSeverity.Verbose, GetBrush(Color.Pink)),
            (LogSeverity.Debug, GetBrush(Color.SandyBrown))
        ];
    }
}

public class LogSourceToBrush : VolteBrushConverter<LogSourceToBrush>
{
    public LogSourceToBrush()
    {
        BrushDefinitions = [
            (LogSource.Volte, GetBrush(Color.LawnGreen)),
            (LogSource.Discord, GetBrush(Color.RoyalBlue)),
            (LogSource.Gateway, GetBrush(Color.RoyalBlue)),
            (LogSource.Service, GetBrush(Color.Gold)),
            (LogSource.Module, GetBrush(Color.LimeGreen)),
            (LogSource.Rest, GetBrush(Color.Red)),
            (LogSource.Sentry, GetBrush(Color.Chartreuse)),
            (LogSource.UI, GetBrush(Color.Crimson)),
            (LogSource.Unknown, GetBrush(Color.Fuchsia))
        ];
    }
}

public abstract class VolteBrushConverter<TConverter> : IValueConverter where TConverter : IValueConverter, new()
{
    private static readonly Lazy<TConverter> _shared = new(() => new());
    public static TConverter Shared => _shared.Value;

    protected HashSet<(object Raw, IBrush Brush)> BrushDefinitions = null!;
    
    public object? Convert(object? value, Type _, object? __, CultureInfo ___) =>
        BrushDefinitions.FindFirst(x => x.Raw.Equals(value))
            .Convert(x => x.Brush);

    public object? ConvertBack(object? value, Type _, object? __, CultureInfo ___) =>
        BrushDefinitions.FindFirst(x => x.Brush.Equals(value))
            .Convert(x => x.Raw);

    protected IBrush GetBrush(Color color) => Brush.Parse(color.ToHexadecimalString());
}