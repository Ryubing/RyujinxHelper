namespace RyuBot.Services;

public class Notes
{
    public readonly string AMDOpenGL = "⚠️ **AMD GPU users should consider using Vulkan graphics backend**";
    public readonly string IntelOpenGL = "⚠️ **Intel GPU users should consider using Vulkan graphics backend**";
    public readonly string IntelMac = "⚠️ **Intel Macs are not supported.**";
    public readonly string Rosetta = "🔴 **Rosetta should be disabled**";
    public readonly string DebugLogs = "⚠️ **Debug logs enabled will have a negative impact on performance**";

    public readonly string MissingLogs = "⚠️ **Logs settings are not default. Consider enabled `Info`, " +
                                         "`Warning`, `Error` and `Guest` logs.**";

    public readonly string DummyAudio = "⚠️ Dummy audio backend, consider changing to SDL2.";
    public readonly string Pptc = "🔴 **PPTC cache should be enabled**";
    public readonly string ShaderCache = "🔴 **Shader cache should be enabled.**";

    public readonly string SoftwareMemory = "🔴 **`Software` setting in Memory Manager Mode will give slower " +
                                            "performance than the default setting of `Host unchecked`.**";

    public readonly string MissingServices = "⚠️ `Ignore Missing Services` being enabled can cause instability.";

    public readonly string FsIntegrity =
        "⚠️ Disabling file integrity checks may cause corrupted dumps to not be detected.";

    public readonly string VSync = "⚠️ V-Sync disabled can cause instability like games running faster than " +
                                   "intended or longer load times.";
    
    public readonly string HashError = "🔴 Dump error detected. Investigate possible bad game/firmware dump issues.";
    public readonly string GameCrashed = "🔴 The game itself crashed, not Ryujinx.";
    public readonly string MissingKeys = "⚠️ Keys or firmware out of date, consider updating them.";

    public readonly string PermissionError = "🔴 File permission error. Consider deleting save directory and " +
                                             "allowing Ryujinx to make a new one.";

    public readonly string FsTargetError = "🔴 Save not found error. Consider starting game without a save file or " +
                                           "using a new save file.";

    public readonly string ServiceError = "⚠️ Consider enabling `Ignore Missing Services` in Ryujinx settings.";
    public readonly string VramError = "⚠️ Consider enabling `Texture Recompression` in Ryujinx settings.";
    public readonly string DefaultProfile = "⚠️ Default user profile in use, consider creating a custom one.";
    public readonly string SaveDataIndex = "🔴 **Save data index for the game may be corrupted.**";
    public readonly string DramSize = "⚠️ `DRAM size` should only be increased for 4K mods.";
    public readonly string BackendThreadingAuto = "🔴 **Graphics Backend Multithreading should be set to `Auto`.**";

    public readonly string CustomRefreshRate = "⚠️ Custom Refresh Rate is experimental, it should only be " +
                                               "enabled in specific cases.";

    public readonly string Firmware =
        "❌ **Nintendo Switch firmware not found**, consider adding your keys and firmware.";

    public readonly string Metal = "⚠️ **The Metal backend is experimental. " +
                                   "If you're experiencing issues, switch to Vulkan or Auto.**";

    public readonly string ShaderCacheCollision =
        "⚠️ Cache collision detected. Investigate possible shader cache issues.";

    public readonly string ShaderCacheCorruption = 
        "⚠️ Cache corruption detected. Investigate possible shader cache issues.";
}

public class FatalErrors
{
    public readonly string Custom = "⚠️ **Custom builds are not officially supported**";

    public readonly string OriginalLdn =
        "**The old Ryujinx LDN build no longer works. Please update to " +
        "[this version](<https://github.com/GreemDev/Ryujinx/releases/latest>). *Yes, it has LDN functionality.***";

    public readonly string Original = 
        "**⚠️ It seems you're still using the original Ryujinx. " +
        "Please update to [this version](<https://github.com/GreemDev/Ryujinx/releases/latest>)," +
        " as that's what this Discord server is for.**";

    public readonly string Mirror = 
        "**It seems you're using the other Ryujinx fork, ryujinx-mirror. " +
        "Please update to [this version](<https://github.com/GreemDev/Ryujinx/releases/latest>), " +
        "as that's what this Discord server is for; or go to their Discord server for support.**";
}