using System.IO;

namespace Volte.Entities;

public class CalledCommandsInfo
{
    public ulong Successful { get; set; }
    public ulong Failed { get; set; }

    public ulong Total => Successful + Failed;

    public void WriteTo(FileStream stream)
    {
        using var bw = new BinaryWriter(stream);
        bw.Write(Successful);
        bw.Write(Failed);
    }

    public void LoadFrom(FileStream stream)
    {
        using var br = new BinaryReader(stream);
        if (stream.Length < (2 * sizeof(ulong)))
            return;
        
        Successful = br.ReadUInt64();
        Failed = br.ReadUInt64();
    }
    
}