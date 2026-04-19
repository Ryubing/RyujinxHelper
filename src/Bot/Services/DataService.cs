namespace RyuBot.Services;

public class DataService : BotService
{
    public static readonly FilePath PersistentRoot = FilePath.Data / "persistent";
    public static readonly FilePath AccountRequestsPath = PersistentRoot / "accountRequests.json";

    private Dictionary<ulong, AccountRequest> _accountRequests = new();

    public void Init()
    {
        if (!PersistentRoot.ExistsAsDirectory)
        {
            PersistentRoot.CreateAsDirectory();
        }

        if (!AccountRequestsPath.ExistsAsFile)
        {
            AccountRequestsPath.WriteAllText("[]");
        }

        _accountRequests = JsonSerializer.Deserialize<AccountRequest[]>(AccountRequestsPath.ReadAllBytes())
            .ToDictionary(x => x.Requestor, x => x);
    }

    public void RegisterAccountRequest(ulong requestor, string email, string username, string reason)
    {
        _accountRequests[requestor] = new AccountRequest
        {
            Requestor = requestor,
            DesiredUsername = username,
            Email = email,
            Reason = reason
        };
        AccountRequestsPath.WriteAllBytes(JsonSerializer.SerializeToUtf8Bytes(_accountRequests.Values));
    }

    public bool HasAlreadyRequestedAccount(ulong requestor) => _accountRequests.ContainsKey(requestor);

    public bool RemoveAccountRequestFor(ulong requestor)
    {
        var result = _accountRequests.Remove(requestor);
        AccountRequestsPath.WriteAllBytes(JsonSerializer.SerializeToUtf8Bytes(_accountRequests.Values));
        return result;
    }
    
    public bool GetAccountRequestFor(ulong requestor, out AccountRequest accountRequest) 
        => _accountRequests.TryGetValue(requestor, out accountRequest);

    public bool RedactAccountRequestFor(ulong requestor)
    {
        if (_accountRequests.TryGetValue(requestor, out var accountRequest))
        {
            _accountRequests[requestor] = new AccountRequest
            {
                Requestor = requestor,
                Email = "[REDACTED]",
                DesiredUsername = "[REDACTED]",
                Reason = accountRequest.Reason
            };

            AccountRequestsPath.WriteAllBytes(JsonSerializer.SerializeToUtf8Bytes(_accountRequests.Values));

            return true;
        }

        return false;
    }
}