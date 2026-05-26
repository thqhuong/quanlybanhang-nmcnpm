namespace quanlybanhang_nmcnpm.Services;

public sealed class UserSessionService : IUserSessionService
{
    public UserSession? CurrentUser { get; private set; }

    public void Start(UserSession userSession)
    {
        CurrentUser = userSession;
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
