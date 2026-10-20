using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeShop.Constants;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Controllers;

[Authorize]
public class ShoesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShoesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? category = null,
        string? brand = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? color = null,
        string? size = null,
        string? search = null,
        string? sort = null)
    {
        var query = _context.Shoes.AsQueryable();

        // Keyword search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(shoe => shoe.Name.ToLower().Contains(s) || shoe.Brand.ToLower().Contains(s));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(shoe => shoe.Category.ToLower() == category.Trim().ToLower());
        }

        // Brand filter
        if (!string.IsNullOrWhiteSpace(brand) && !brand.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(shoe => shoe.Brand.ToLower() == brand.Trim().ToLower());
        }

        // Color filter
        if (!string.IsNullOrWhiteSpace(color) && !color.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(shoe => shoe.Color != null && shoe.Color.ToLower() == color.Trim().ToLower());
        }

        // Price range filter
        if (minPrice.HasValue)
        {
            query = query.Where(shoe => shoe.Price >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(shoe => shoe.Price <= maxPrice.Value);
        }

        // Sorting
        query = sort switch
        {
            "price_asc" => query.OrderBy(s => s.Price),
            "price_desc" => query.OrderByDescending(s => s.Price),
            "name_asc" => query.OrderBy(s => s.Name),
            "newest" => query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.CreatedAt)
        };

        var shoes = await query.ToListAsync();

        // Set ViewBag for active states
        ViewBag.CurrentCategory = category ?? "All";
        ViewBag.CurrentBrand = brand ?? "All";
        ViewBag.CurrentColor = color ?? "All";
        ViewBag.CurrentMinPrice = minPrice;
        ViewBag.CurrentMaxPrice = maxPrice;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentSort = sort ?? "newest";
        ViewBag.CurrentSize = size ?? "All";

        // Provide metadata for filter badges & counts
        var allShoes = await _context.Shoes.ToListAsync();
        ViewBag.AllShoes = allShoes;
        ViewBag.AvailableBrands = allShoes.Select(s => s.Brand).Distinct().OrderBy(b => b).ToList();
        ViewBag.AvailableCategories = new List<string> { "All", "Running", "Basketball", "Lifestyle" };

        return View(shoes);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var shoe = await _context.Shoes.FirstOrDefaultAsync(m => m.Id == id);
        if (shoe == null) return NotFound();

        return View(shoe);
    }

    [HttpGet]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    public IActionResult Create() => View();

    [HttpPost]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Brand,Name,Category,Color,Price,ImageUrl")] Shoe shoe)
    {
        if (string.IsNullOrWhiteSpace(shoe.Category))
        {
            shoe.Category = "Lifestyle";
        }
        if (string.IsNullOrWhiteSpace(shoe.Color))
        {
            shoe.Color = "Black";
        }

        if (!ModelState.IsValid) return View(shoe);

        shoe.CreatedAt = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(shoe.ImageUrl))
        {
            shoe.ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=800&q=80";
        }

        _context.Add(shoe);
        await _context.SaveChangesAsync();
        TempData["StatusMessage"] = $"Successfully added '{shoe.Brand} {shoe.Name}' to product inventory!";
        return RedirectToAction("Index", "AdminDashboard");
    }

    [HttpGet]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var shoe = await _context.Shoes.FindAsync(id);
        if (shoe == null) return NotFound();

        return View(shoe);
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Brand,Name,Category,Color,Price,ImageUrl")] Shoe shoe)
    {
        if (id != shoe.Id) return NotFound();

        if (string.IsNullOrWhiteSpace(shoe.Category))
        {
            shoe.Category = "Lifestyle";
        }
        if (string.IsNullOrWhiteSpace(shoe.Color))
        {
            shoe.Color = "Black";
        }

        if (!ModelState.IsValid) return View(shoe);

        try
        {
            var existingShoe = await _context.Shoes.AsNoTracking().FirstOrDefaultAsync(e => e.Id == shoe.Id);
            if (existingShoe == null) return NotFound();

            shoe.CreatedAt = existingShoe.CreatedAt;
            if (string.IsNullOrWhiteSpace(shoe.ImageUrl))
            {
                shoe.ImageUrl = existingShoe.ImageUrl;
            }

            _context.Update(shoe);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Successfully updated '{shoe.Brand} {shoe.Name}'!";
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Shoes.AnyAsync(e => e.Id == shoe.Id))
                return NotFound();
            throw;
        }

        return RedirectToAction("Index", "AdminDashboard");
    }

    [HttpGet]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var shoe = await _context.Shoes.FirstOrDefaultAsync(m => m.Id == id);
        if (shoe == null) return NotFound();

        return View(shoe);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Policy = Policies.RequireInventoryManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var shoe = await _context.Shoes.FindAsync(id);
        if (shoe != null)
        {
            var name = $"{shoe.Brand} {shoe.Name}";
            _context.Shoes.Remove(shoe);
            await _context.SaveChangesAsync();
            TempData["StatusMessage"] = $"Successfully deleted '{name}' from inventory.";
        }

        return RedirectToAction("Index", "AdminDashboard");
    }
}
