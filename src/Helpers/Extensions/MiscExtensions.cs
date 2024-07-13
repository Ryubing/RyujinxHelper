using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Discord.Color;

namespace Gommon;

public partial class Extensions
{
    public static System.Drawing.Color AsColor(this Vector3 vec3)
        => System.Drawing.Color.FromArgb(255,
            (int)(Math.Min(vec3.X, 1) * 255),
            (int)(Math.Min(vec3.Y, 1) * 255),
            (int)(Math.Min(vec3.Z, 1) * 255));

    public static Vector3 AsVec3(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f);
    
    public static Vector4 AsVec4(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    
    public static bool LengthEquals(this Stream stream, long exactLength) => stream.Length == exactLength;
    
    public static MemoryStream CreateColorImage(this Rgba32 color, int width = 125, int height = 200) => new MemoryStream().Apply(ms =>
    {
        using var image = new Image<Rgba32>(width, height);
        image.Mutate(a => a.BackgroundColor(color));
        image.SaveAsPng(ms);
        ms.Position = 0;
    });

    public static Rgba32 ToRgba32(this Color color) => new(color.R, color.G, color.B);

    public static Task WarnAsync(this SocketGuildUser member, VolteContext ctx, string reason)
        => ModerationModule.WarnAsync(ctx.User, ctx.GuildData, member,
            ctx.Services.GetRequiredService<DatabaseService>(), reason);
}