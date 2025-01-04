namespace RyuBot.Helpers;

public static class StringHelper
{
    public static string Capitalize(this string value)
    {
        if (value == string.Empty)
            return string.Empty;
        
        var firstChar = value[0];
        var rest = value[1..];

        return $"{char.ToUpper(firstChar)}{rest}";
    }
}