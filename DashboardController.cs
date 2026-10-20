using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Data;
using ShoeShop.Models;
using ShoeShop.ViewModels.Account;

namespace ShoeShop.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Challenge();

        // Retrieve all orders for this user (both by UserId and CustomerEmail)
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == user.Id || (user.Email != null && o.CustomerEmail == user.Email))
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            City = user.City,
            PostalCode = user.PostalCode,
            MemberSince = user.CreatedAt,
            Orders = orders
        };

        return View(viewModel);
    }
}
