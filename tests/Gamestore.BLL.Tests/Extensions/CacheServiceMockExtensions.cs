using Gamestore.BLL.Interfaces;
using Moq;

namespace Gamestore.BLL.Tests.Extensions;

public static class CacheServiceMockExtensions
{
    public static void SetupGetCached<T>(
        this Mock<ICacheService> mock,
        string key,
        T resultInstance)
    {
        mock.Setup(x => x.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, Task<T>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultInstance);
    }

    public static void SetupGetNotCached<T>(
        this Mock<ICacheService> mock,
        string key)
    {
        mock.Setup(x => x.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, Task<T>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, Task<T>>, TimeSpan?, CancellationToken>(
                async (_, factory, _, token) => await factory(token));
    }

    public static void VerifyRemoveCalled(
        this Mock<ICacheService> mock,
        string key,
        Func<Times> times)
    {
        mock.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), times);
    }

    public static void VerifyRemoveCalledOnce(
        this Mock<ICacheService> mock,
        string key)
    {
        mock.VerifyRemoveCalled(key, Times.Once);
    }
}
