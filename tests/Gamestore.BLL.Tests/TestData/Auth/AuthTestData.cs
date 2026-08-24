using Gamestore.BLL.DTOs.Auth;

namespace Gamestore.BLL.Tests.TestData.Auth;

public static class AuthTestData
{
    public static TokenDto GetToken()
    {
        return new TokenDto("valid.jwt.token");
    }

    public static LoginRequest GetLoginRequest(string? login = null)
    {
        return new LoginRequest
        {
            Login = login ?? "TestUser",
            Password = "Password",
        };
    }

    public static LoginRequest GetInvalidLoginRequest()
    {
        return new LoginRequest
        {
            Login = string.Empty,
            Password = string.Empty,
        };
    }
}