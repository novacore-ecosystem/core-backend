using NovaCore.BuildingBlock.SharedKernel.Constants;

namespace NovaCore.Auth.Persistence.Storage.Seeders;

/// <summary>
/// One permission's seed metadata - its key plus the initial English (mandatory) and Vietnamese
/// (optional) display names <see cref="PermissionCatalogSeeder"/> translates a newly-created
/// definition with.
/// </summary>
public sealed record PermissionSeed(string Key, string EnglishDisplayName, string? VietnameseDisplayName);

/// <summary>
/// The explicit inventory of every permission definition to seed, one <see cref="CreatePerm"/> call
/// per permission.
/// </summary>
/// <remarks>
/// Deliberately explicit rather than reflected off <c>PermissionRegistry</c> - a permission's
/// structural facts (group, allowed providers) are registry-owned and code-first, but its display
/// translations are not, and reflection cannot invent them. Adding a language later only needs
/// another optional <c>CreatePerm</c> parameter, not a redesign of this catalog.
/// </remarks>
public static class PermissionSeedCatalog
{
    public static IReadOnlyList<PermissionSeed> All { get; } =
    [
        CreatePerm(Permissions.Root, en: "Root (Full System Access)", vi: "Toàn quyền hệ thống (Root)"),
        CreatePerm(Permissions.User, en: "Platform User", vi: "Người dùng nền tảng"),

        CreatePerm(Permissions.Role.View, en: "View Roles", vi: "Xem vai trò"),
        CreatePerm(Permissions.Role.Manage, en: "Manage Roles", vi: "Quản lý vai trò"),
        CreatePerm(Permissions.Role.Full, en: "Full Role Access", vi: "Toàn quyền vai trò"),

        CreatePerm(Permissions.Account.View, en: "View Account Authorization", vi: "Xem phân quyền tài khoản"),
        CreatePerm(Permissions.Account.Manage, en: "Manage Account Authorization", vi: "Quản lý phân quyền tài khoản"),
        CreatePerm(Permissions.Account.Full, en: "Full Account Authorization Access", vi: "Toàn quyền phân quyền tài khoản"),

        CreatePerm(Permissions.Permission.View, en: "View Permissions", vi: "Xem quyền hạn"),
        CreatePerm(Permissions.Permission.Manage, en: "Manage Permissions", vi: "Quản lý quyền hạn"),
        CreatePerm(Permissions.Permission.Full, en: "Full Permission Access", vi: "Toàn quyền quyền hạn"),

        CreatePerm(Permissions.System.MessagingView, en: "View System Messaging", vi: "Xem hàng đợi tin nhắn hệ thống"),
        CreatePerm(Permissions.System.MessagingRequeue, en: "Requeue System Messages", vi: "Xử lý lại tin nhắn hệ thống"),
        CreatePerm(Permissions.System.Full, en: "Full System Access", vi: "Toàn quyền hệ thống"),

        CreatePerm(Permissions.Tenant.View, en: "View Tenants", vi: "Xem tổ chức thuê bao"),
        CreatePerm(Permissions.Tenant.Manage, en: "Manage Tenants", vi: "Quản lý tổ chức thuê bao"),
        CreatePerm(Permissions.Tenant.RotateClient, en: "Rotate Tenant Client Keys", vi: "Xoay khóa client thuê bao"),
        CreatePerm(Permissions.Tenant.Full, en: "Full Tenant Access", vi: "Toàn quyền tổ chức thuê bao"),

        CreatePerm(Permissions.App.View, en: "View Apps", vi: "Xem ứng dụng"),
        CreatePerm(Permissions.App.Manage, en: "Manage Apps", vi: "Quản lý ứng dụng"),
        CreatePerm(Permissions.App.AssignUsers, en: "Assign Users to Apps", vi: "Gán người dùng vào ứng dụng"),

        CreatePerm(Permissions.Inventory.View, en: "View Inventory", vi: "Xem tồn kho"),
        CreatePerm(Permissions.Inventory.StockMove, en: "Move Stock", vi: "Chuyển kho"),
        CreatePerm(Permissions.Inventory.Adjust, en: "Adjust Stock", vi: "Điều chỉnh tồn kho"),
        CreatePerm(Permissions.Inventory.Receive, en: "Receive Stock", vi: "Nhập kho"),
        CreatePerm(Permissions.Inventory.Transfer, en: "Transfer Stock", vi: "Chuyển kho nội bộ"),
        CreatePerm(Permissions.Inventory.CycleCount, en: "Cycle Count Inventory", vi: "Kiểm kê định kỳ"),
        CreatePerm(Permissions.Inventory.Full, en: "Full Inventory Access", vi: "Toàn quyền tồn kho"),

        CreatePerm(Permissions.Warehouse.View, en: "View Warehouses", vi: "Xem kho hàng"),
        CreatePerm(Permissions.Warehouse.Manage, en: "Manage Warehouses", vi: "Quản lý kho hàng"),
        CreatePerm(Permissions.Warehouse.Full, en: "Full Warehouse Access", vi: "Toàn quyền kho hàng"),

        CreatePerm(Permissions.Notification.View, en: "View Notifications", vi: "Xem thông báo"),
        CreatePerm(Permissions.Notification.Manage, en: "Manage Notifications", vi: "Quản lý thông báo"),
        CreatePerm(Permissions.Notification.ChannelToggle, en: "Enable/Disable Notification Channels", vi: "Bật/tắt kênh thông báo"),
        CreatePerm(Permissions.Notification.ChannelConfigure, en: "Configure Notification Channels", vi: "Cấu hình kênh thông báo"),
        CreatePerm(Permissions.Notification.CampaignManage, en: "Manage Notification Campaigns", vi: "Quản lý chiến dịch thông báo"),
        CreatePerm(Permissions.Notification.Send, en: "Send Notifications", vi: "Gửi thông báo"),
        CreatePerm(Permissions.Notification.Full, en: "Full Notification Access", vi: "Toàn quyền thông báo"),

        CreatePerm(Permissions.Order.View, en: "View Orders", vi: "Xem đơn hàng"),
        CreatePerm(Permissions.Order.Manage, en: "Manage Orders", vi: "Quản lý đơn hàng"),
        CreatePerm(Permissions.Order.Fulfill, en: "Fulfill Orders", vi: "Xử lý hoàn tất đơn hàng"),
        CreatePerm(Permissions.Order.Delete, en: "Delete Orders", vi: "Xóa đơn hàng"),
        CreatePerm(Permissions.Order.CreateOnBehalf, en: "Create Orders on Behalf of Customers", vi: "Tạo đơn hàng thay khách hàng"),
        CreatePerm(Permissions.Order.Full, en: "Full Order Access", vi: "Toàn quyền đơn hàng"),

        CreatePerm(Permissions.Product.Manage, en: "Manage Products", vi: "Quản lý sản phẩm"),
        CreatePerm(Permissions.Product.Reindex, en: "Rebuild Product Search Index", vi: "Xây dựng lại chỉ mục tìm kiếm sản phẩm"),
        CreatePerm(Permissions.Product.Full, en: "Full Product Access", vi: "Toàn quyền sản phẩm"),

        CreatePerm(Permissions.Users.View, en: "View Users", vi: "Xem người dùng"),
        CreatePerm(Permissions.Users.Manage, en: "Manage Users", vi: "Quản lý người dùng"),
        CreatePerm(Permissions.Users.Reindex, en: "Rebuild User Search Index", vi: "Xây dựng lại chỉ mục tìm kiếm người dùng"),
        CreatePerm(Permissions.Users.Full, en: "Full User Access", vi: "Toàn quyền người dùng"),

        CreatePerm(Permissions.Audit.View, en: "View Audit Logs", vi: "Xem nhật ký kiểm toán"),
        CreatePerm(Permissions.Audit.Full, en: "Full Audit Access", vi: "Toàn quyền kiểm toán"),
    ];

    private static PermissionSeed CreatePerm(string key, string en, string? vi = null) => new(key, en, vi);
}
