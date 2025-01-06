using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Gommon;
using Color = System.Drawing.Color;

namespace RyuBot.UI.Converters;

public class PlayabilityStatusConverter : IValueConverter
{
    private static readonly Lazy<PlayabilityStatusConverter> _shared = new(() => new());
    public static PlayabilityStatusConverter Shared => _shared.Value;

    public object Convert(object? value, Type _, object? __, CultureInfo ___) =>
        value?.ToString()?.ToLower() switch
        {
            "nothing" or "boots" or "menus" => Brush.Parse(Color.Red.ToHexadecimalString()),
            "ingame" => Brush.Parse(Color.Yellow.ToHexadecimalString()),
            _ => Brush.Parse(Color.ForestGreen.ToHexadecimalString())
        };

    public object ConvertBack(object? value, Type _, object? __, CultureInfo ___) 
        => throw new NotSupportedException();
}