using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Constants;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: /Orders
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == user.Id || (user.Email != null && o.CustomerEmail == user.Email))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    // GET: /Orders/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }

        // Ensure user owns this order (or is Admin)
        bool isOwner = order.UserId == user.Id ||
            (!string.IsNullOrEmpty(user.Email) && string.Equals(order.CustomerEmail, user.Email, StringComparison.OrdinalIgnoreCase)) ||
            User.IsInRole(Roles.Admin);

        if (!isOwner)
        {
            TempData["ErrorMessage"] = "You do not have permission to view this order.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    // POST: /Orders/CancelOrder
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelOrder(int orderId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // 1. Retrieve the order with its order items
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToReferrerOrDashboard(returnUrl);
        }

        // 2. Ensure the order belongs to the logged-in user
        bool isOwner = order.UserId == user.Id ||
            (!string.IsNullOrEmpty(user.Email) && string.Equals(order.CustomerEmail, user.Email, StringComparison.OrdinalIgnoreCase));

        if (!isOwner)
        {
            _logger.LogWarning("User {UserId} attempted to cancel unauthorized Order #{OrderId}", user.Id, orderId);
            TempData["ErrorMessage"] = "You do not have permission to cancel this order.";
            return RedirectToReferrerOrDashboard(returnUrl);
        }

        // 3. Check order status: Only allow cancellation if the status is "Pending" or "Processing"
        // If the status is "Shipped" or "Delivered", prevent cancellation and show an error message
        if (order.Status.StartsWith("Shipped", StringComparison.OrdinalIgnoreCase) ||
            order.Status.StartsWith("Delivered", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = $"Order #{order.OrderNumber} cannot be cancelled because it has already been {order.Status.ToLower()}.";
            return RedirectToReferrerOrDashboard(returnUrl);
        }

        bool isPendingOrProcessing = order.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                                     order.Status.StartsWith("Pending", StringComparison.OrdinalIgnoreCase) ||
                                     order.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase) ||
                                     order.Status.StartsWith("Processing", StringComparison.OrdinalIgnoreCase);

        if (!isPendingOrProcessing)
        {
            TempData["ErrorMessage"] = $"Order #{order.OrderNumber} cannot be cancelled because its current status is '{order.Status}'.";
            return RedirectToReferrerOrDashboard(returnUrl);
        }

        // 4. When an order is successfully cancelled:
        //    - Update its status to "Cancelled"
        //    - Automatically restore the shoe stock quantity in the database by adding back the ordered quantities
        order.Status = "Cancelled";
        if (string.IsNullOrEmpty(order.CancellationReason))
        {
            order.CancellationReason = "Cancelled by Customer";
        }

        foreach (var item in order.OrderItems)
        {
            var shoe = await _context.Shoes.FindAsync(item.ShoeId);
            if (shoe != null)
            {
                shoe.StockQuantity += item.Quantity;
                _logger.LogInformation("Restored {Qty} units of shoe '{ShoeName}' (ID: {ShoeId}). New stock: {NewStock}",
                    item.Quantity, item.ShoeName, shoe.Id, shoe.StockQuantity);
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Order #{OrderNumber} (ID: {OrderId}) was successfully cancelled by user {UserId}.",
            order.OrderNumber, order.Id, user.Id);

        TempData["StatusMessage"] = $"Order #{order.OrderNumber} has been successfully cancelled and shoe stock was restored.";
        return RedirectToReferrerOrDashboard(returnUrl);
    }

    private IActionResult RedirectToReferrerOrDashboard(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        var referer = Request.Headers.Referer.ToString();
        if (!string.IsNullOrEmpty(referer) && (referer.Contains("/Dashboard") || referer.Contains("/Orders")))
        {
            return Redirect(referer);
        }

        return RedirectToAction(nameof(Index));
    }
}
