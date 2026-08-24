namespace Gamestore.WebApi.Constants;

public static class PagePolicy
{
    public static Dictionary<string, string> GamePagePolicy { get; } = new()
    {
        { "Game", CustomPolicyNames.ReadOnlyPolicy },
        { "Games", CustomPolicyNames.ReadOnlyPolicy },
        { "AddGame", CustomPolicyNames.CanManageGames },
        { "UpdateGame", CustomPolicyNames.CanManageGames },
        { "DeleteGame", CustomPolicyNames.CanManageGames },
    };

    public static Dictionary<string, string> GenrePagePolicy { get; } = new()
    {
        { "Genre", CustomPolicyNames.ReadOnlyPolicy },
        { "Genres", CustomPolicyNames.ReadOnlyPolicy },
        { "AddGenre", CustomPolicyNames.CanManageGenres },
        { "UpdateGenre", CustomPolicyNames.CanManageGenres },
        { "DeleteGenre", CustomPolicyNames.CanManageGenres },
    };

    public static Dictionary<string, string> PlatformPagePolicy { get; } = new()
    {
        { "Platform", CustomPolicyNames.ReadOnlyPolicy },
        { "Platforms", CustomPolicyNames.ReadOnlyPolicy },
        { "AddPlatform", CustomPolicyNames.CanManagePlatforms },
        { "UpdatePlatform", CustomPolicyNames.CanManagePlatforms },
        { "DeletePlatform", CustomPolicyNames.CanManagePlatforms },
    };

    public static Dictionary<string, string> PublisherPagePolicy { get; } = new()
    {
        { "Publisher", CustomPolicyNames.ReadOnlyPolicy },
        { "Publishers", CustomPolicyNames.ReadOnlyPolicy },
        { "AddPublisher", CustomPolicyNames.CanManagePublishers },
        { "UpdatePublisher", CustomPolicyNames.CanManagePublishers },
        { "DeletePublisher", CustomPolicyNames.CanManagePublishers },
    };

    public static Dictionary<string, string> CommentPagePolicy { get; } = new()
    {
        { "Comment", CustomPolicyNames.ReadOnlyPolicy },
        { "Comments", CustomPolicyNames.ReadOnlyPolicy },
        { "AddComment", CustomPolicyNames.CanCommentGames },
        { "ReplyComment", CustomPolicyNames.CanCommentGames },
        { "QuoteComment", CustomPolicyNames.CanCommentGames },
        { "DeleteComment", CustomPolicyNames.CanManageGameComments },
        { "BanComment", CustomPolicyNames.CanBanUserFromCommenting },
    };

    public static Dictionary<string, string> OrderPagePolicy { get; } = new()
    {
        { "Order", CustomPolicyNames.ReadOnlyPolicy },
        { "Orders", CustomPolicyNames.CanViewOrdersHistory },
        { "MakeOrder", CustomPolicyNames.CanBuyGames },
        { "UpdateOrder", CustomPolicyNames.CanEditOrders },
        { "ShipOrder", CustomPolicyNames.CanChangeOrderStatusToShipped },
        { "History",  CustomPolicyNames.CanViewOrdersHistory },
    };

    public static Dictionary<string, string> CartPagePolicy { get; } = new()
    {
        { "Basket", CustomPolicyNames.CanBuyGames },
        { "Buy", CustomPolicyNames.CanBuyGames },
    };

    public static Dictionary<string, string> UserPagePolicy { get; } = new()
    {
        { "User", CustomPolicyNames.CanManageUsers },
        { "Users", CustomPolicyNames.CanManageUsers },
        { "AddUser", CustomPolicyNames.CanManageUsers },
        { "UpdateUser", CustomPolicyNames.CanManageUsers },
        { "DeleteUser", CustomPolicyNames.CanManageUsers },
    };

    public static Dictionary<string, string> RolePagePolicy { get; } = new()
    {
        { "Role", CustomPolicyNames.CanManageRoles },
        { "Roles", CustomPolicyNames.CanManageRoles },
        { "AddRole", CustomPolicyNames.CanManageRoles },
        { "UpdateRole", CustomPolicyNames.CanManageRoles },
        { "DeleteRole", CustomPolicyNames.CanManageRoles },
    };

    public static Dictionary<string, string> AllPolicies { get; } = GamePagePolicy
        .Concat(GenrePagePolicy)
        .Concat(PlatformPagePolicy)
        .Concat(PublisherPagePolicy)
        .Concat(CommentPagePolicy)
        .Concat(OrderPagePolicy)
        .Concat(CartPagePolicy)
        .Concat(UserPagePolicy)
        .Concat(RolePagePolicy)
        .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
}