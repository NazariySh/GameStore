namespace Gamestore.WebApi.Constants;

public static class CustomPolicyNames
{
    public const string ReadOnlyPolicy = nameof(ReadOnlyPolicy);

    public const string CanManageGames = nameof(CanManageGames);

    public const string CanCommentGames = nameof(CanCommentGames);
    public const string CanManageGameComments = nameof(CanManageGameComments);
    public const string CanBanUserFromCommenting = nameof(CanBanUserFromCommenting);

    public const string CanManageGenres = nameof(CanManageGenres);
    public const string CanManagePlatforms = nameof(CanManagePlatforms);
    public const string CanManagePublishers = nameof(CanManagePublishers);

    public const string CanBuyGames = nameof(CanBuyGames);
    public const string CanEditOrders = nameof(CanEditOrders);
    public const string CanViewOrdersHistory = nameof(CanViewOrdersHistory);
    public const string CanChangeOrderStatusToShipped = nameof(CanChangeOrderStatusToShipped);

    public const string CanManageUsers = nameof(CanManageUsers);
    public const string CanManageUserProfile = nameof(CanManageUserProfile);
    public const string CanManageRoles = nameof(CanManageRoles);
}