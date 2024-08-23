// ReSharper disable MemberCanBePrivate.Global


namespace Volte.Helpers;

using static IconVariant;

public enum IconVariant { Solid, Regular, Light, Thin }

public static class FontAwesome
{
    public static readonly FontAwesomeIcon Check = Solid.Named("check");
    public static readonly FontAwesomeIcon Message = Regular.Named("message");
    public static readonly FontAwesomeIcon RightToBracket = Regular.Named("right-to-bracket");
    public static readonly FontAwesomeIcon Brush = Solid.Named("brush");
    public static readonly FontAwesomeIcon Copy = Regular.Named("copy");
    
    public static FontAwesomeIcon Named(this IconVariant defaultVariant, string name) => new(name, defaultVariant);
}

public readonly record struct FontAwesomeIcon
{
    public FontAwesomeIcon() : this("", (IconVariant)int.MaxValue) {}
        
    public FontAwesomeIcon(string name, IconVariant defaultVariant)
    {
        FaName = "fa-" + name;
        DefaultVariant = defaultVariant;
    }
        
    public readonly string FaName;
    public readonly IconVariant DefaultVariant;
        
    public string Format(IconVariant variant = IconVariant.Regular) => $"fa-{Enum.GetName(variant)?.ToLower() ?? "regular"} {FaName}";
        
    public string Solid => Format(IconVariant.Solid);
    public string Regular => Format();
    public string Light => Format(IconVariant.Light);
    public string Thin => Format(IconVariant.Thin);

    public override string ToString() => Format(DefaultVariant);
    public static implicit operator string(FontAwesomeIcon icon) => icon.Format(icon.DefaultVariant);
}

