namespace ShoeShop.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class CustomClaims
{
    public const string CanManageInventory = "CanManageInventory";
}

public static class Policies
{
    public const string RequireInventoryManager = "RequireInventoryManager";
}
