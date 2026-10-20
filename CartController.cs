using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeShop.Data;
using ShoeShop.Models;

namespace ShoeShop.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CartController> _logger;
    private const string CartSessionKey = "Cart";
    private const string CartCountKey = "CartCount";

    public CartController(ApplicationDbContext context, ILogger<CartController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Cart
    [HttpGet]
    public IActionResult Index()
    {
        var cart = GetCartFromSession();
        return View(cart);
    }

    // POST: /Cart/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int shoeId, string? size = null)
    {
        var shoe = await _context.Shoes.FindAsync(shoeId);
        if (shoe == null)
        {
            return NotFound("Shoe not found.");
        }

        var cart = GetCartFromSession();
        var selectedSize = string.IsNullOrWhiteSpace(size) ? "Standard" : size.Trim();

        // Check if item (with same shoe ID and size) is already in the cart
        var existingItem = cart.FirstOrDefault(item => item.ShoeId == shoeId && item.Size == selectedSize);

        if (existingItem != null)
        {
            existingItem.Quantity++;
        }
        else
        {
            cart.Add(new CartItem
            {
                ShoeId = shoe.Id,
                Name = $"{shoe.Brand} {shoe.Name}",
                Price = shoe.Price,
                ImageUrl = shoe.ImageUrl,
                Quantity = 1,
                Size = selectedSize
            });
        }

        // Save updated cart back into Session
        SaveCartToSession(cart);

        TempData["StatusMessage"] = $"Added '{shoe.Brand} {shoe.Name}' ({selectedSize}) to your shopping cart!";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/UpdateQuantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateQuantity(int shoeId, string? size, int change)
    {
        var cart = GetCartFromSession();
        var selectedSize = string.IsNullOrWhiteSpace(size) ? "Standard" : size.Trim();
        var item = cart.FirstOrDefault(i => i.ShoeId == shoeId && i.Size == selectedSize);

        if (item != null)
        {
            item.Quantity += change;
            if (item.Quantity <= 0)
            {
                cart.Remove(item);
                TempData["StatusMessage"] = $"Removed '{item.Name}' from your cart.";
            }
            SaveCartToSession(cart);
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Remove
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int shoeId, string? size = null)
    {
        var cart = GetCartFromSession();
        var selectedSize = string.IsNullOrWhiteSpace(size) ? "Standard" : size.Trim();
        var itemToRemove = cart.FirstOrDefault(i => i.ShoeId == shoeId && i.Size == selectedSize);

        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
            SaveCartToSession(cart);
            TempData["StatusMessage"] = $"Removed '{itemToRemove.Name}' from your cart.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Cart/Clear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        HttpContext.Session.Remove(CartCountKey);
        TempData["StatusMessage"] = "Your shopping cart has been cleared.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Cart/Checkout
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Checkout([FromServices] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        var cart = GetCartFromSession();
        if (!cart.Any())
        {
            TempData["StatusMessage"] = "Your shopping cart is empty. Please add items before proceeding to checkout.";
            return RedirectToAction(nameof(Index));
        }

        var model = new ShoeShop.ViewModels.CheckoutViewModel
        {
            CartItems = cart
        };

        // Prefill shipping profile from authenticated customer account
        var user = await userManager.GetUserAsync(User);
        if (user != null)
        {
            model.FullName = user.FullName;
            model.Email = user.Email ?? string.Empty;
            model.Phone = user.PhoneNumber ?? string.Empty;
            var addressParts = new[] { user.Address, user.City, user.PostalCode }
                .Where(s => !string.IsNullOrWhiteSpace(s));
            model.ShippingAddress = string.Join(", ", addressParts);
        }

        return View(model);
    }

    // POST: /Cart/Checkout
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(
        ShoeShop.ViewModels.CheckoutViewModel model,
        [FromServices] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
    {
        var cart = GetCartFromSession();
        if (!cart.Any())
        {
            TempData["StatusMessage"] = "Your shopping cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        // 2. Perform model validation. If invalid, return view with existing session cart data
        if (!ModelState.IsValid)
        {
            model.CartItems = cart;
            return View(model);
        }

        // Customer MUST be authenticated to purchase
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // 3. Handle payment scenarios based on model.PaymentMethod
        string orderStatus;
        switch (model.PaymentMethod?.Trim())
        {
            case "COD":
                orderStatus = "Pending - Cash on Delivery";
                _logger.LogInformation("Order received via COD. Status: {Status}. Customer: {Customer}", orderStatus, model.FullName);
                break;

            case "PayPal":
                // Simulate successful fake PayPal authorization
                orderStatus = "Paid via PayPal";
                _logger.LogInformation("Simulated PayPal payment authorized. Status: {Status}. Customer: {Customer}", orderStatus, model.FullName);
                break;

            case "QRCode":
                orderStatus = "Paid via QR Code";
                _logger.LogInformation("Simulated QR Code payment verified. Status: {Status}. Customer: {Customer}", orderStatus, model.FullName);
                break;

            default:
                orderStatus = "Pending Payment";
                _logger.LogWarning("Unknown payment method '{Method}' received.", model.PaymentMethod);
                break;
        }

        // Generate Order ID & calculate total amount
        var orderNumber = "ORD-" + Random.Shared.Next(100000, 999999);
        var totalAmount = cart.Sum(item => item.Total);

        // Create and persist order and order items to database linked to the authenticated user
        var order = new Order
        {
            OrderNumber = orderNumber,
            UserId = user.Id,
            CustomerName = model.FullName.Trim(),
            CustomerEmail = model.Email.Trim(),
            CustomerPhone = model.Phone.Trim(),
            ShippingAddress = model.ShippingAddress.Trim(),
            PaymentMethod = model.PaymentMethod ?? "COD",
            Status = orderStatus,
            TotalAmount = totalAmount,
            CreatedAt = DateTime.UtcNow,
            OrderItems = cart.Select(c => new OrderItem
            {
                ShoeId = c.ShoeId,
                ShoeName = c.Name,
                Size = string.IsNullOrWhiteSpace(c.Size) ? "Standard" : c.Size,
                UnitPrice = c.Price,
                Quantity = c.Quantity,
                ImageUrl = c.ImageUrl
            }).ToList()
        };

        // Deduct ordered quantities from shoe stock
        foreach (var cartItem in cart)
        {
            var shoe = await _context.Shoes.FindAsync(cartItem.ShoeId);
            if (shoe != null)
            {
                shoe.StockQuantity = Math.Max(0, shoe.StockQuantity - cartItem.Quantity);
            }
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 4. Clear the shopping cart session
        HttpContext.Session.Remove(CartSessionKey);
        HttpContext.Session.Remove(CartCountKey);

        // 5. Redirect to OrderConfirmation action, passing the generated Order ID and payment details
        return RedirectToAction(nameof(OrderConfirmation), new
        {
            orderId = order.OrderNumber,
            customerName = order.CustomerName,
            shippingAddress = order.ShippingAddress,
            totalAmount = order.TotalAmount,
            paymentMethod = order.PaymentMethod,
            status = order.Status
        });
    }

    // GET: /Cart/OrderConfirmation
    [HttpGet]
    [Authorize]
    public IActionResult OrderConfirmation(
        string? orderId,
        string? customerName,
        string? shippingAddress,
        decimal? totalAmount,
        string? paymentMethod,
        string? status)
    {
        ViewBag.OrderId = orderId ?? ("ORD-" + Random.Shared.Next(100000, 999999));
        ViewBag.CustomerName = !string.IsNullOrWhiteSpace(customerName) ? customerName : (User.Identity?.Name ?? "Valued Customer");
        ViewBag.ShippingAddress = !string.IsNullOrWhiteSpace(shippingAddress) ? shippingAddress : "Standard Delivery Address";
        ViewBag.TotalAmount = totalAmount ?? 0m;
        ViewBag.PaymentMethod = paymentMethod ?? "COD";
        ViewBag.OrderStatus = status ?? "Pending - Cash on Delivery";

        return View();
    }

    // ----------------------------------------------------
    // HELPER METHODS
    // ----------------------------------------------------
    private List<CartItem> GetCartFromSession()
    {
        var json = HttpContext.Session.GetString(CartSessionKey);
        if (string.IsNullOrEmpty(json))
        {
            return new List<CartItem>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }
        catch
        {
            return new List<CartItem>();
        }
    }

    private void SaveCartToSession(List<CartItem> cart)
    {
        var json = JsonSerializer.Serialize(cart);
        HttpContext.Session.SetString(CartSessionKey, json);

        // Update the total item count badge in session
        var totalCount = cart.Sum(item => item.Quantity);
        HttpContext.Session.SetInt32(CartCountKey, totalCount);
    }
}
