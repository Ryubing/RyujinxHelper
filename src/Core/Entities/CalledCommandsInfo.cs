using System.IO;

namespace Volte.Entities;

public class CalledCommandsInfo
{
    public static readonly FilePath CalledCommandsFile = FilePath.Data / "commandstats.bin";

    private static bool _isInitialized;
    private static CalledCommandsInfo _instance = new();
    public static CalledCommandsInfo Instance
    {
        get
        {
            if (!_isInitialized)
            {
                Load();
                _isInitialized = true;
            }
            return _instance;
        }
    }

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
    
    public static void Load()
    {
        if (!CalledCommandsFile.ExistsAsFile)
        {
            CalledCommandsFile.Create();
            Save();
        }
        else
        {
            using var readStream = CalledCommandsFile.OpenRead();
            _instance.Read(readStream);
        }
    }

    public static void Save()
    {
        using var fileStream = CalledCommandsFile.OpenWrite();
        _instance.Write(fileStream);
    }
    
    public static void UpdateSaved(MessageService messageService)
    {
        _instance.Successful += messageService.SuccessfulCommandCalls;
        _instance.Failed += messageService.FailedCommandCalls;
        messageService.ResetCalledCommands();
        Save();
    }
    
}