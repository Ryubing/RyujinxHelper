using Silk.NET.SDL;

namespace Volte.UI;

// https://github.com/adobe/imgui/blob/master/imgui_spectrum.h
public static class Spectrum
{
    public static unsafe ThemedColors* Dark
    {
        get
        {
            fixed (DarkThemedColors* ptr = &DarkThemedColors.Instance)
                return (ThemedColors*)ptr;
        }
    }

    public static unsafe ThemedColors* Light
    {
        get
        {
            fixed (LightThemedColors* ptr = &LightThemedColors.Instance)
                return (ThemedColors*)ptr;
        }
    }

    public class DarkThemedColors : ThemedColors
    {
        internal static DarkThemedColors Instance = new();
        internal override Color Gray50 => Color(0x252525);
        internal override Color Gray75 => Color(0x2F2F2F);
        internal override Color Gray100 => Color(0x323232);
        internal override Color Gray200 => Color(0x393939);
        internal override Color Gray300 => Color(0x3E3E3E);
        internal override Color Gray400 => Color(0x4D4D4D);
        internal override Color Gray500 => Color(0x5C5C5C);
        internal override Color Gray600 => Color(0x7B7B7B);
        internal override Color Gray700 => Color(0x999999);
        internal override Color Gray800 => Color(0xCDCDCD);
        internal override Color Gray900 => Color(0xFFFFFF);
        internal override Color Blue400 => Color(0x2680EB);
        internal override Color Blue500 => Color(0x378EF0);
        internal override Color Blue600 => Color(0x4B9CF5);
        internal override Color Blue700 => Color(0x5AA9FA);
        internal override Color Red400 => Color(0xE34850);
        internal override Color Red500 => Color(0xEC5B62);
        internal override Color Red600 => Color(0xF76D74);
        internal override Color Red700 => Color(0xFF7B82);
        internal override Color Orange400 => Color(0xE68619);
        internal override Color Orange500 => Color(0xF29423);
        internal override Color Orange600 => Color(0xF9A43F);
        internal override Color Orange700 => Color(0xFFB55B);
        internal override Color Green400 => Color(0x2D9D78);
        internal override Color Green500 => Color(0x33AB84);
        internal override Color Green600 => Color(0x39B990);
        internal override Color Green700 => Color(0x3FC89C);
        internal override Color Indigo400 => Color(0x6767EC);
        internal override Color Indigo500 => Color(0x7575F1);
        internal override Color Indigo600 => Color(0x8282F6);
        internal override Color Indigo700 => Color(0x9090FA);
        internal override Color Celery400 => Color(0x44B556);
        internal override Color Celery500 => Color(0x4BC35F);
        internal override Color Celery600 => Color(0x51D267);
        internal override Color Celery700 => Color(0x58E06F);
        internal override Color Magenta400 => Color(0xD83790);
        internal override Color Magenta500 => Color(0xE2499D);
        internal override Color Magenta600 => Color(0xEC5AAA);
        internal override Color Magenta700 => Color(0xF56BB7);
        internal override Color Yellow400 => Color(0xDFBF00);
        internal override Color Yellow500 => Color(0xEDCC00);
        internal override Color Yellow600 => Color(0xFAD900);
        internal override Color Yellow700 => Color(0xFFE22E);
        internal override Color Fuchsia400 => Color(0xC038CC);
        internal override Color Fuchsia500 => Color(0xCF3EDC);
        internal override Color Fuchsia600 => Color(0xD951E5);
        internal override Color Fuchsia700 => Color(0xE366EF);
        internal override Color Seafoam400 => Color(0x1B959A);
        internal override Color Seafoam500 => Color(0x20A3A8);
        internal override Color Seafoam600 => Color(0x23B2B8);
        internal override Color Seafoam700 => Color(0x26C0C7);
        internal override Color Chartreuse400 => Color(0x85D044);
        internal override Color Chartreuse500 => Color(0x8EDE49);
        internal override Color Chartreuse600 => Color(0x9BEC54);
        internal override Color Chartreuse700 => Color(0xA3F858);
        internal override Color Purple400 => Color(0x9256D9);
        internal override Color Purple500 => Color(0x9D64E1);
        internal override Color Purple600 => Color(0xA873E9);
        internal override Color Purple700 => Color(0xB483F0);
    }

    public class LightThemedColors : ThemedColors
    {
        internal static LightThemedColors Instance = new();
        internal override Color Gray50 => Color(0xFFFFFF);
        internal override Color Gray75 => Color(0xFAFAFA);
        internal override Color Gray100 => Color(0xF5F5F5);
        internal override Color Gray200 => Color(0xEAEAEA);
        internal override Color Gray300 => Color(0xE1E1E1);
        internal override Color Gray400 => Color(0xCACACA);
        internal override Color Gray500 => Color(0xB3B3B3);
        internal override Color Gray600 => Color(0x8E8E8E);
        internal override Color Gray700 => Color(0x707070);
        internal override Color Gray800 => Color(0x4B4B4B);
        internal override Color Gray900 => Color(0x2C2C2C);
        internal override Color Blue400 => Color(0x2680EB);
        internal override Color Blue500 => Color(0x1473E6);
        internal override Color Blue600 => Color(0x0D66D0);
        internal override Color Blue700 => Color(0x095ABA);
        internal override Color Red400 => Color(0xE34850);
        internal override Color Red500 => Color(0xD7373F);
        internal override Color Red600 => Color(0xC9252D);
        internal override Color Red700 => Color(0xBB121A);
        internal override Color Orange400 => Color(0xE68619);
        internal override Color Orange500 => Color(0xDA7B11);
        internal override Color Orange600 => Color(0xCB6F10);
        internal override Color Orange700 => Color(0xBD640D);
        internal override Color Green400 => Color(0x2D9D78);
        internal override Color Green500 => Color(0x268E6C);
        internal override Color Green600 => Color(0x12805C);
        internal override Color Green700 => Color(0x107154);
        internal override Color Indigo400 => Color(0x6767EC);
        internal override Color Indigo500 => Color(0x5C5CE0);
        internal override Color Indigo600 => Color(0x5151D3);
        internal override Color Indigo700 => Color(0x4646C6);
        internal override Color Celery400 => Color(0x44B556);
        internal override Color Celery500 => Color(0x3DA74E);
        internal override Color Celery600 => Color(0x379947);
        internal override Color Celery700 => Color(0x318B40);
        internal override Color Magenta400 => Color(0xD83790);
        internal override Color Magenta500 => Color(0xCE2783);
        internal override Color Magenta600 => Color(0xBC1C74);
        internal override Color Magenta700 => Color(0xAE0E66);
        internal override Color Yellow400 => Color(0xDFBF00);
        internal override Color Yellow500 => Color(0xD2B200);
        internal override Color Yellow600 => Color(0xC4A600);
        internal override Color Yellow700 => Color(0xB79900);
        internal override Color Fuchsia400 => Color(0xC038CC);
        internal override Color Fuchsia500 => Color(0xB130BD);
        internal override Color Fuchsia600 => Color(0xA228AD);
        internal override Color Fuchsia700 => Color(0x93219E);
        internal override Color Seafoam400 => Color(0x1B959A);
        internal override Color Seafoam500 => Color(0x16878C);
        internal override Color Seafoam600 => Color(0x0F797D);
        internal override Color Seafoam700 => Color(0x096C6F);
        internal override Color Chartreuse400 => Color(0x85D044);
        internal override Color Chartreuse500 => Color(0x7CC33F);
        internal override Color Chartreuse600 => Color(0x73B53A);
        internal override Color Chartreuse700 => Color(0x6AA834);
        internal override Color Purple400 => Color(0x9256D9);
        internal override Color Purple500 => Color(0x864CCC);
        internal override Color Purple600 => Color(0x7A42BF);
        internal override Color Purple700 => Color(0x6F38B1);
    }


    public static class Static
    {
        internal static Color None = Color(0x000000);
        internal static Color Gray200 = Color(0xF4F4F4);
        internal static Color Gray300 = Color(0xEAEAEA);
        internal static Color Gray400 = Color(0xD3D3D3);
        internal static Color Gray500 = Color(0xBCBCBC);
        internal static Color Gray600 = Color(0x959595);
        internal static Color Gray700 = Color(0x767676);
        internal static Color Gray800 = Color(0x505050);
        internal static Color Gray900 = Color(0x323232);
        internal static Color Blue400 = Color(0x378EF0);
        internal static Color Blue500 = Color(0x2680EB);
        internal static Color Blue600 = Color(0x1473E6);
        internal static Color Blue700 = Color(0x0D66D0);
        internal static Color Red400 = Color(0xEC5B62);
        internal static Color Red500 = Color(0xE34850);
        internal static Color Red600 = Color(0xD7373F);
        internal static Color Red700 = Color(0xC9252D);
        internal static Color Orange400 = Color(0xF29423);
        internal static Color Orange500 = Color(0xE68619);
        internal static Color Orange600 = Color(0xDA7B11);
        internal static Color Orange700 = Color(0xCB6F10);
        internal static Color Green400 = Color(0x33AB84);
        internal static Color Green500 = Color(0x2D9D78);
        internal static Color Green600 = Color(0x268E6C);
        internal static Color Green700 = Color(0x12805C);
    }

    private static Color Color(uint raw)
    {
        var r = (raw >> 16) & 0xFF;
        var g = (raw >> 8) & 0xFF;
        var b = (raw >> 0) & 0xFF;
        return new Color((byte)r, (byte)g, (byte)b);
    }
}

public abstract class ThemedColors
{
    internal abstract Color Gray50 { get; }
    internal abstract Color Gray75 { get; }
    internal abstract Color Gray100 { get; }
    internal abstract Color Gray200 { get; }
    internal abstract Color Gray300 { get; }
    internal abstract Color Gray400 { get; }
    internal abstract Color Gray500 { get; }
    internal abstract Color Gray600 { get; }
    internal abstract Color Gray700 { get; }
    internal abstract Color Gray800 { get; }
    internal abstract Color Gray900 { get; }
    internal abstract Color Blue400 { get; }
    internal abstract Color Blue500 { get; }
    internal abstract Color Blue600 { get; }
    internal abstract Color Blue700 { get; }
    internal abstract Color Red400 { get; }
    internal abstract Color Red500 { get; }
    internal abstract Color Red600 { get; }
    internal abstract Color Red700 { get; }
    internal abstract Color Orange400 { get; }
    internal abstract Color Orange500 { get; }
    internal abstract Color Orange600 { get; }
    internal abstract Color Orange700 { get; }
    internal abstract Color Green400 { get; }
    internal abstract Color Green500 { get; }
    internal abstract Color Green600 { get; }
    internal abstract Color Green700 { get; }
    internal abstract Color Indigo400 { get; }
    internal abstract Color Indigo500 { get; }
    internal abstract Color Indigo600 { get; }
    internal abstract Color Indigo700 { get; }
    internal abstract Color Celery400 { get; }
    internal abstract Color Celery500 { get; }
    internal abstract Color Celery600 { get; }
    internal abstract Color Celery700 { get; }
    internal abstract Color Magenta400 { get; }
    internal abstract Color Magenta500 { get; }
    internal abstract Color Magenta600 { get; }
    internal abstract Color Magenta700 { get; }
    internal abstract Color Yellow400 { get; }
    internal abstract Color Yellow500 { get; }
    internal abstract Color Yellow600 { get; }
    internal abstract Color Yellow700 { get; }
    internal abstract Color Fuchsia400 { get; }
    internal abstract Color Fuchsia500 { get; }
    internal abstract Color Fuchsia600 { get; }
    internal abstract Color Fuchsia700 { get; }
    internal abstract Color Seafoam400 { get; }
    internal abstract Color Seafoam500 { get; }
    internal abstract Color Seafoam600 { get; }
    internal abstract Color Seafoam700 { get; }
    internal abstract Color Chartreuse400 { get; }
    internal abstract Color Chartreuse500 { get; }
    internal abstract Color Chartreuse600 { get; }
    internal abstract Color Chartreuse700 { get; }
    internal abstract Color Purple400 { get; }
    internal abstract Color Purple500 { get; }
    internal abstract Color Purple600 { get; }
    internal abstract Color Purple700 { get; }
}