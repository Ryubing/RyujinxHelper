namespace RyuBot.Services;

public enum RyujinxVersion
{
    Stable,
    Canary,
    Pr,
    OriginalProject,
    OriginalProjectLdn,
    Mirror,
    Custom
}

public enum CommonError
{
    ShaderCacheCollision,
    DumpHash,
    ShaderCacheCorruption,
    UpdateKeys,
    FilePermissions,
    FileNotFound,
    MissingServices,
    VulkanOutOfMemory
}