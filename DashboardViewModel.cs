using ShoeShop.Models;

namespace ShoeShop.ViewModels.Account;

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public DateTime MemberSince { get; set; }
    public List<Order> Orders { get; set; } = new();
}
