namespace Gamestore.Domain.Exceptions;

public class UserBannedException : ForbiddenException
{
    public UserBannedException(string userName)
        : base($"User '{userName}' is banned and cannot perform this action.")
    {
    }
}