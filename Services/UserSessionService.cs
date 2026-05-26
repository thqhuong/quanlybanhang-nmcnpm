namespace quanlybanhang_nmcnpm.Services;

public sealed class UserSessionService : IUserSessionService
{
    public UserSession? CurrentUser { get; private set; }

    public void Start(UserSession userSession)
    {
        CurrentUser = userSession;
    }

    public bool IsInRole(params string[] roles)
    {
        return CurrentUser is not null
            && roles.Any(role => string.Equals(CurrentUser.Role, role, StringComparison.OrdinalIgnoreCase));
    }

    public void Clear()
    {
        CurrentUser = null;
    }
}
