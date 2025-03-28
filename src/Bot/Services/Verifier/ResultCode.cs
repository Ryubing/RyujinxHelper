namespace RyuBot.Services;

public enum ResultCode : sbyte
{
    Success,
    InvalidInput,
    InvalidTokenLength,
    TokenIsZeroes,
    ChecksumFailure,
    SerialChecksumFailure,
    NotSwitch,
    NotConsumerDevice,
    ExpiredToken,
    BackendDownForMaintenance = -1,
}