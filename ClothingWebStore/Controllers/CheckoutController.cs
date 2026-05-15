using ClothingWebStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingWebStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly WebShopContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CheckoutController(
            WebShopContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(
     string billingStreet,
     string billingCity,
     string billingPostalCode,
     string billingCountry,

     string shippingStreet,
     string shippingCity,
     string shippingPostalCode,
     string shippingCountry,

     bool sameAddress)
        {
            var user = await _userManager.GetUserAsync(User);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // BILLING ADDRESS 

            var billingAddress = new Address
            {
                UserId = user.Id,
                Street = billingStreet,
                City = billingCity,
                PostalCode = billingPostalCode,
                Country = billingCountry
            };

            _context.Addresses.Add(billingAddress);

            //  SHIPPING ADDRESS 

            Address shippingAddress;

            if (!sameAddress)
            {
                shippingAddress = billingAddress;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(shippingStreet) ||
                    string.IsNullOrWhiteSpace(shippingCity) ||
                    string.IsNullOrWhiteSpace(shippingPostalCode) ||
                    string.IsNullOrWhiteSpace(shippingCountry))
                {
                    ModelState.AddModelError("", "Shipping address is incomplete.");
                    return RedirectToAction("Index");
                }

                shippingAddress = new Address
                {
                    UserId = user.Id,
                    Street = shippingStreet,
                    City = shippingCity,
                    PostalCode = shippingPostalCode,
                    Country = shippingCountry
                };

                _context.Addresses.Add(shippingAddress);
            }

            // SAVE ADDRESSES FIRST
            await _context.SaveChangesAsync();

            // totals

            decimal subtotal = cart.CartItems.Sum(i => i.Product.Price * i.Quantity);
            decimal vat = subtotal * 0.20m;
            decimal total = subtotal + vat;

            // create order

            var order = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.Now,
                BillingAddressId = billingAddress.AddressId,
                ShippingAddressId = shippingAddress.AddressId,
                TotalPrice = total,
                Vat = vat
            };

            _context.Orders.Add(order);

            // SAVE ORDER FIRST SO IT GETS AN ID
            await _context.SaveChangesAsync();

            // order items

            foreach (var item in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Product.Price
                };

                _context.OrderItems.Add(orderItem);
            }

            //clear cart

            _context.CartItems.RemoveRange(cart.CartItems);

            // FINAL SAVE
            await _context.SaveChangesAsync();

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}