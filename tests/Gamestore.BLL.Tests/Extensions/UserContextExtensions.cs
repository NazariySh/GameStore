using Gamestore.DAL.Services.Interfaces;
using Moq;

namespace Gamestore.BLL.Tests.Extensions;

public static class UserContextExtensions
{
    public static void SetupHasPermission(this Mock<IUserContext> mock, string permission, bool canView)
    {
        mock.Setup(x => x.HasPermission(permission)).Returns(canView);
    }
}