namespace Gamestore.Domain.Constants;

public static class Permissions
{
    public const string ManageGames = nameof(ManageGames);
    public const string ViewDeletedGames = nameof(ViewDeletedGames);
    public const string EditDeletedGames = nameof(EditDeletedGames);

    public const string CommentGames = nameof(CommentGames);
    public const string ManageGameComments = nameof(ManageGameComments);
    public const string ManageCommentsForDeletedGames = nameof(ManageCommentsForDeletedGames);
    public const string BanUserFromCommenting = nameof(BanUserFromCommenting);

    public const string ManageGenres = nameof(ManageGenres);
    public const string ManagePlatforms = nameof(ManagePlatforms);
    public const string ManagePublishers = nameof(ManagePublishers);

    public const string BuyGames = nameof(BuyGames);
    public const string EditOrders = nameof(EditOrders);
    public const string ViewOrdersHistory = nameof(ViewOrdersHistory);
    public const string EditOrdersFromHistory = nameof(EditOrdersFromHistory);
    public const string ChangeOrderStatusToShipped = nameof(ChangeOrderStatusToShipped);

    public const string ManageUsers = nameof(ManageUsers);
    public const string ManageUserProfile = nameof(ManageUserProfile);
    public const string ManageRoles = nameof(ManageRoles);
}