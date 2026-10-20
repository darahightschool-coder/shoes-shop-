using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Constants;
using ShoeShop.Data;
using ShoeShop.Models;
using ShoeShop.ViewModels.Admin;

namespace ShoeShop.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminDashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public AdminDashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var usersInUserRole = await _userManager.GetUsersInRoleAsync(Roles.User);
        var usersInAdminRole = await _userManager.GetUsersInRoleAsync(Roles.Admin);

        var recentUsers = await _userManager.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(10)
            .ToListAsync();

        var totalOrders = await _context.Orders.CountAsync();
        var orderAmounts = await _context.Orders.Select(o => o.TotalAmount).ToListAsync();
        var totalRevenue = orderAmounts.Sum();
        var pendingOrdersCount = await _context.Orders
            .CountAsync(o => o.Status.Contains("Pending") || o.Status == "Processing");

        var recentOrders = await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .ToListAsync();

        var shoes = await _context.Shoes
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalCustomers = usersInUserRole.Count,
            TotalAdmins = usersInAdminRole.Count,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrdersCount = pendingOrdersCount,
            TotalShoes = shoes.Count,
            Shoes = shoes,
            RecentOrders = recentOrders,
            RecentUsers = recentUsers
        };

        return View(viewModel);
    }

    // GET: /AdminDashboard/Orders
    [HttpGet]
    public async Task<IActionResult> Orders()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // GET: /AdminDashboard/OrderDetails/5
    [HttpGet]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Orders));
        }

        return View(order);
    }

    // POST: /AdminDashboard/AdminCancelOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> AdminCancelOrder(int orderId, string cancellationReason)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectBackOrToDashboard();
        }

        if (order.Status == "Cancelled")
        {
            TempData["ErrorMessage"] = $"Order #{order.OrderNumber} is already cancelled.";
            return RedirectBackOrToDashboard();
        }

        // 2. Administrators can cancel orders at ANY stage (Pending, Processing, Shipped, etc.)
        // 3. Save the selected cancellationReason to the Order entity
        var reason = string.IsNullOrWhiteSpace(cancellationReason) ? "Cancelled by Administrator" : cancellationReason.Trim();
        order.CancellationReason = reason;
        order.Status = "Cancelled";

        // 4. Restore shoe inventory stock count in the database upon cancellation
        foreach (var item in order.OrderItems)
        {
            var shoe = await _context.Shoes.FindAsync(item.ShoeId);
            if (shoe != null)
            {
                shoe.StockQuantity += item.Quantity;
            }
        }

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = $"Order #{order.OrderNumber} was cancelled by Admin. Reason: \"{reason}\". Stock quantities restored.";
        return RedirectBackOrToDashboard();
    }

    // POST: /AdminDashboard/UpdateOrderStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound("Order not found.");
        }

        var oldStatus = order.Status;
        order.Status = newStatus;

        // If manually marked Cancelled through status dropdown without modal, restore stock
        if (newStatus == "Cancelled" && oldStatus != "Cancelled")
        {
            if (string.IsNullOrEmpty(order.CancellationReason))
            {
                order.CancellationReason = "Cancelled by Administrator";
            }

            foreach (var item in order.OrderItems)
            {
                var shoe = await _context.Shoes.FindAsync(item.ShoeId);
                if (shoe != null)
                {
                    shoe.StockQuantity += item.Quantity;
                }
            }
        }

        await _context.SaveChangesAsync();

        TempData["StatusMessage"] = $"Order #{order.OrderNumber} status updated from '{oldStatus}' to '{newStatus}'.";
        return RedirectBackOrToDashboard();
    }

    private IActionResult RedirectBackOrToDashboard()
    {
        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer) && referer.Contains("/AdminDashboard"))
        {
            return Redirect(referer);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleInventoryClaim(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("User not found.");

        var claims = await _userManager.GetClaimsAsync(user);
        var existingClaim = claims.FirstOrDefault(c => c.Type == CustomClaims.CanManageInventory);

        if (existingClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, existingClaim);
            TempData["StatusMessage"] = $"Revoked Inventory Management claim from {user.FullName}.";
        }
        else
        {
            var newClaim = new Claim(CustomClaims.CanManageInventory, "true");
            await _userManager.AddClaimAsync(user, newClaim);
            TempData["StatusMessage"] = $"Granted Inventory Management claim to {user.FullName}.";
        }

        return RedirectToAction(nameof(Index));
    }
}
