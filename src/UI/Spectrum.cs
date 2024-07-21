namespace Volte.UI;

internal static class Spectrum
{
    internal static class Static
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

    internal static Color Gray50 = Color(0x252525);
    internal static Color Gray75 = Color(0x2F2F2F);
    internal static Color Gray100 = Color(0x323232);
    internal static Color Gray200 = Color(0x393939);
    internal static Color Gray300 = Color(0x3E3E3E);
    internal static Color Gray400 = Color(0x4D4D4D);
    internal static Color Gray500 = Color(0x5C5C5C);
    internal static Color Gray600 = Color(0x7B7B7B);
    internal static Color Gray700 = Color(0x999999);
    internal static Color Gray800 = Color(0xCDCDCD);
    internal static Color Gray900 = Color(0xFFFFFF);
    internal static Color Blue400 = Color(0x2680EB);
    internal static Color Blue500 = Color(0x378EF0);
    internal static Color Blue600 = Color(0x4B9CF5);
    internal static Color Blue700 = Color(0x5AA9FA);
    internal static Color Red400 = Color(0xE34850);
    internal static Color Red500 = Color(0xEC5B62);
    internal static Color Red600 = Color(0xF76D74);
    internal static Color Red700 = Color(0xFF7B82);
    internal static Color Orange400 = Color(0xE68619);
    internal static Color Orange500 = Color(0xF29423);
    internal static Color Orange600 = Color(0xF9A43F);
    internal static Color Orange700 = Color(0xFFB55B);
    internal static Color Green400 = Color(0x2D9D78);
    internal static Color Green500 = Color(0x33AB84);
    internal static Color Green600 = Color(0x39B990);
    internal static Color Green700 = Color(0x3FC89C);
    internal static Color Indigo400 = Color(0x6767EC);
    internal static Color Indigo500 = Color(0x7575F1);
    internal static Color Indigo600 = Color(0x8282F6);
    internal static Color Indigo700 = Color(0x9090FA);
    internal static Color Celery400 = Color(0x44B556);
    internal static Color Celery500 = Color(0x4BC35F);
    internal static Color Celery600 = Color(0x51D267);
    internal static Color Celery700 = Color(0x58E06F);
    internal static Color Magenta400 = Color(0xD83790);
    internal static Color Magenta500 = Color(0xE2499D);
    internal static Color Magenta600 = Color(0xEC5AAA);
    internal static Color Magenta700 = Color(0xF56BB7);
    internal static Color Yellow400 = Color(0xDFBF00);
    internal static Color Yellow500 = Color(0xEDCC00);
    internal static Color Yellow600 = Color(0xFAD900);
    internal static Color Yellow700 = Color(0xFFE22E);
    internal static Color Fuchsia400 = Color(0xC038CC);
    internal static Color Fuchsia500 = Color(0xCF3EDC);
    internal static Color Fuchsia600 = Color(0xD951E5);
    internal static Color Fuchsia700 = Color(0xE366EF);
    internal static Color Seafoam400 = Color(0x1B959A);
    internal static Color Seafoam500 = Color(0x20A3A8);
    internal static Color Seafoam600 = Color(0x23B2B8);
    internal static Color Seafoam700 = Color(0x26C0C7);
    internal static Color Chartreuse400 = Color(0x85D044);
    internal static Color Chartreuse500 = Color(0x8EDE49);
    internal static Color Chartreuse600 = Color(0x9BEC54);
    internal static Color Chartreuse700 = Color(0xA3F858);
    internal static Color Purple400 = Color(0x9256D9);
    internal static Color Purple500 = Color(0x9D64E1);
    internal static Color Purple600 = Color(0xA873E9);
    internal static Color Purple700 = Color(0xB483F0);
    
    private static Color Color(uint raw)
    {
        var r = (raw >> 16) & 0xFF;
        var g = (raw >> 8) & 0xFF;
        var b = (raw >> 0) & 0xFF;
        return new Color((int)r, (int)g, (int)b);
    }
}