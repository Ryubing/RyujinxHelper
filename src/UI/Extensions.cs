using System;
using System.IO;
using System.Numerics;
using System.Text;
using Gommon;
using Silk.NET.SDL;

namespace Volte.UI;

public static class Extensions
{
    #region Color
    public static System.Drawing.Color AsColor(this Vector3 vec3)
        => System.Drawing.Color.FromArgb(255,
            (int)(vec3.X.CoerceAtMost(1) * 255),
            (int)(vec3.Y.CoerceAtMost(1) * 255),
            (int)(vec3.Z.CoerceAtMost(1) * 255));

    public static Vector3 AsVec3(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f);
    
    public static Vector4 AsVec4(this System.Drawing.Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    
    public static Vector3 AsVec3(this Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f);
    
    public static Vector4 AsVec4(this Color color)
        => new(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    
    #endregion Color
}

public static class Buffers
{
    public static void CopyBytesTo(this string str, Span<byte> span, Encoding encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var bytes = encoding.GetBytes(str);
        
        bytes.CopyTo(span);
        span[bytes.Length] = 0; //null terminator
    }
    
    public static unsafe void CopyBytesTo(this string str, byte* buffer, int bufferSize) 
        => CopyBytesTo(str, new Span<byte>(buffer, bufferSize));

    /**
     * Reads the <see cref="Stream"/> into a <see cref="Span{T}"/> of bytes.
     * Will throw by default if the read byte count is less than the <see cref="Stream"/>'s length.
     * <param name="stream">The stream to read from.</param>
     * <param name="throwOnUnderRead">Whether to throw if the read byte count is less than the stream's length.</param>
     * <param name="fromStart">Whether to seek to the beginning of the stream before reading.</param>
     */
    public static Span<byte> ToSpan(this Stream stream, bool throwOnUnderRead = true, bool fromStart = true)
    {
        if (fromStart)
            stream.Seek(0, SeekOrigin.Begin);
        
        var data = new byte[stream.Length];
        
        // no using is a deliberate choice here; the user is responsible for disposing the stream,
        // plus that might not be a desired side effect of calling this.
        var read = new BinaryReader(stream).Read(data, 0, data.Length);
        if (throwOnUnderRead && read != data.Length)
            throw new InvalidDataException("Could not read all bytes from the stream.");
        
        return data;
    }
    
    public static unsafe void CopyBytesFromString(byte* buffer, int bufferSize, string str)
        => str.CopyBytesTo(buffer, bufferSize);
}