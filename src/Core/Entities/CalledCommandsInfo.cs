namespace Volte.Core.Entities;

public class CalledCommandsInfo
{
    public required ulong Successful { get; set; }
    public required ulong Failed { get; set; }

    public ulong Total => Successful + Failed;
}