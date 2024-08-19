using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using System.Text;
using Discord;
using Color = System.Drawing.Color;

// ReSharper disable InconsistentNaming

namespace Volte.UI.Converters;

public class LogSeverityToBrush : IValueConverter
{
    private static readonly BrushConverter _brushConverter = new();
    
    private static readonly object? _default = Brushes.Transparent;
    private static readonly object? _verbose = _brushConverter.ConvertFromInvariantString(Hex(Color.SpringGreen));
    private static readonly object? _debug = _brushConverter.ConvertFromInvariantString(Hex(Color.SandyBrown));
    private static readonly object? _warning = _brushConverter.ConvertFromInvariantString(Hex(Color.Yellow));
    private static readonly object? _error = _brushConverter.ConvertFromInvariantString(Hex(Color.DarkRed));
    private static readonly object? _critical = _brushConverter.ConvertFromInvariantString(Hex(Color.Maroon));
    
    private static string Hex(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

    private static readonly Lazy<LogSeverityToBrush> _shared = new(() => new());
    public static LogSeverityToBrush Shared => _shared.Value;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogSeverity logLevel) 
            return logLevel switch 
            {
                LogSeverity.Debug => _debug,
                LogSeverity.Warning => _warning,
                LogSeverity.Error => _error,
                LogSeverity.Critical => _critical,
                LogSeverity.Verbose => _verbose,
                _ => _default
            };

        return _default;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == _verbose)
            return LogSeverity.Verbose;
        
        if (value == _debug)
            return LogSeverity.Debug;
        
        if (value == _warning)
            return LogSeverity.Warning;
        
        if (value == _error)
            return LogSeverity.Error;
        
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (value == _critical)
            return LogSeverity.Critical;

        return LogSeverity.Info;
    }
}