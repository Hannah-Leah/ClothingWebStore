using ClothingWebStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingWebStore.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly WebShopContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(
            WebShopContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // USER: own orders
        // ADMIN: all orders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            IQueryable<Order> orders = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress);

            // If not admin → only own orders
            if (!User.IsInRole("Admin"))
            {
                orders = orders.Where(o => o.UserId == user.Id);
            }

            return View(await orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync());
        }

        // ORDER DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .Include(o => o.BillingAddress)
                .Include(o => o.ShippingAddress)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Normal users can only see their own orders
            if (!User.IsInRole("Admin") && order.UserId != user.Id)
            {
                return Forbid();
            }

            return View(order);
        }

        // ADMIN ONLY
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
