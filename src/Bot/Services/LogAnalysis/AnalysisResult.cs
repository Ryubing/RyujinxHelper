namespace RyuBot.Services;

public struct LogAnalysis
{
    private LogAnalysisService _service;

    public LogAnalysis(string rawLogText, LogAnalysisService service)
    {
        RawLogContent = rawLogText;
        _service = service;
    }
    
    public string RawLogContent { get; }
    public List<string> LogErrors;
    public HardwareInfo Hardware;
    public EmulatorInfo EmulatorInfo;
    public GameInfo Game;
    public Settings Settings;
}

public class HardwareInfo
{
    public string Cpu { get; set; }
    public string Gpu { get; set; }
    public string Ram { get; set; }
    public string Os { get; set; }
}

public class EmulatorInfo
{
    public (RyujinxVersion VersionType, string VersionString) Version { get; set; }
    public string Firmware { get; set; }
    public string EnabledLogs { get; set; }
}

public class GameInfo
{
    public string Name { get; set; }
    public string AppId { get; set; }
    public string AppIdBids { get; set; }
    public string BuildIDs { get; set; }
    public string Errors { get; set; }
    public string Mods { get; set; }
    public string Cheats { get; set; }
}

public class Settings
{
    public string AudioBackend { get; set; }
    public bool BackendThreading { get; set; }
    public bool Docked { get; set; }
    public string DramSize { get; set; }
    public bool FsIntegrityChecks { get; set; }
    public string GraphicsBackend { get; set; }
    public bool IgnoreMissingServices { get; set; }
    public string MemoryManager { get; set; }
    public bool Pptc { get; set; }
    public bool ShaderCache { get; set; }
    public string VSyncMode { get; set; }
    public string Hypervisor { get; set; }
    public string ResScale { get; set; }
    public string AnisotropicFiltering { get; set; }
    public string AspectRatio { get; set; }
    public bool TextureRecompression { get; set; }
    public string CustomVSyncInterval { get; set; }
    public string MultiplayerMode { get; set; }
}