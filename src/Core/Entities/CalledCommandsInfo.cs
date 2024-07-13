using System.IO;

namespace Volte.Entities;

public class CalledCommandsInfo
{
    public ulong Successful { get; set; }
    public ulong Failed { get; set; }

    public ulong Total => Successful + Failed;

    public void Write(Stream stream)
    {
        using var bw = new BinaryWriter(stream);
        bw.Write(Successful);
        bw.Write(Failed);
    }

    public void Read(Stream stream)
    {
        using var br = new BinaryReader(stream);
        if (!stream.LengthEquals(sizeof(ulong) * 2) /* 16 */) return;
        
        Successful = br.ReadUInt64();
        Failed = br.ReadUInt64();
    }
    
}