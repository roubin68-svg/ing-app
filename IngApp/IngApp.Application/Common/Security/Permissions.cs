namespace IngApp.Application.Common.Security;

public static class Permissions
{
    public static class Products
    {
        public const string ViewOwn = "Product.ViewOwn";
        public const string ViewAll = "Product.ViewAll";
        public const string Create = "Product.Create";
        public const string Update = "Product.Update";
        public const string Delete = "Product.Delete";
    }

    public static class ProductCategories
    {
        public const string Manage = "ProductCategory.Manage";
    }

    public static class Users
    {
        public const string View = "User.View";
        public const string Manage = "User.Manage";
    }

    public static class Roles
    {
        public const string View = "Role.View";
        public const string Manage = "Role.Manage";
    }

    public static class PermissionsModule
    {
        public const string View = "Permission.View";
        public const string Manage = "Permission.Manage";
    }

    public static class Menus
    {
        public const string View = "Menu.View";
        public const string Manage = "Menu.Manage";
    }

    public static class Offers
    {
        public const string Manage = "Offer.Manage";
    }

    public static class Visitors
    {
        public const string View = "Visitor.View";
        public const string Manage = "Visitor.Manage";
    }

    public static class Financial
    {
        public const string Manage = "Financial.Manage";
        public const string CommissionRuleManage = "Financial.CommissionRule.Manage";
        public const string WalletManage = "Financial.Wallet.Manage";
    }
}
