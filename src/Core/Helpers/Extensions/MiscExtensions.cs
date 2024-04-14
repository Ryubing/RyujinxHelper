using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Discord.Color;

namespace Gommon
{
    public partial class Extensions
    {
        public static MemoryStream CreateColorImage(this Rgba32 color) => new MemoryStream().Apply(ms =>
        {
            using var image = new Image<Rgba32>(125, 200);
            image.Mutate(a => a.BackgroundColor(color));
            image.SaveAsPng(ms);
            ms.Position = 0;
        });

        public static Rgba32 ToRgba32(this Color color) => new Rgba32(color.R, color.G, color.B);

        public static Task WarnAsync(this SocketGuildUser member, VolteContext ctx, string reason)
            => ModerationModule.WarnAsync(ctx.User, ctx.GuildData, member,
                ctx.Services.GetRequiredService<DatabaseService>(), reason);
    }
}