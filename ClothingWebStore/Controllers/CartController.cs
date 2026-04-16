using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingWebStore.Models;

public class CartController : Controller
{
    private readonly WebShopContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CartController(WebShopContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

  
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        return View(cart);
    }


    [HttpPost]
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto data)
    {
        int productId = data.ProductId;

        var user = await _userManager.GetUserAsync(User);

        var product = await _context.Products.FindAsync(productId);

        if (product == null || product.Stock <= 0)
        {
            return Json(new { success = false, message = "Out of stock" });
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        if (cart == null)
        {
            cart = new Cart { UserId = user.Id };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
        }

        var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

        if (cartItem != null)
        {
            cartItem.Quantity++;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.CartId,
                ProductId = productId,
                Quantity = 1
            });
        }

        //  REDUCE STOCK
        product.Stock--;

        await _context.SaveChangesAsync();

        return Json(new { success = true });

        
    }

    // remove from cart

    [HttpPost]
    public async Task<IActionResult> RemoveFromCart(int productId)
    {
        var user = await _userManager.GetUserAsync(User);

        var cartItem = await _context.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.ProductId == productId && ci.Cart.UserId == user.Id);

        if (cartItem != null)
        {
            var product = await _context.Products.FindAsync(productId);

            // RESTORE STOCK
            product.Stock += cartItem.Quantity;

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true });
    }



    public class AddToCartDto
    {
        public int ProductId { get; set; }
    }
}