using ShoeShop.Models;

namespace ShoeShop.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrdersCount { get; set; }
    public int TotalShoes { get; set; }
    public List<Shoe> Shoes { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public List<ApplicationUser> RecentUsers { get; set; } = new();
}
