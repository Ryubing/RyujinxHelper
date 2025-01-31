namespace RyuBot.Services;

public enum ResultCode
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
}