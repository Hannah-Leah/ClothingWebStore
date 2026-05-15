//using ClothingWebStore.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ClothingWebStore.Controllers
//{


//    public class AspNetUsersController : Controller
//    {
//        private readonly WebShopContext _context;

//        public AspNetUsersController(WebShopContext context)
//        {
//            _context = context;
//        }

//        // GET: AspNetUsers
//        public async Task<IActionResult> Index()
//        {
//            return View(await _context.AspNetUsers.ToListAsync());
//        }

//        // GET: AspNetUsers/Details/5
//        public async Task<IActionResult> Details(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var aspNetUser = await _context.AspNetUsers
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (aspNetUser == null)
//            {
//                return NotFound();
//            }

//            return View(aspNetUser);
//        }

//        // GET: AspNetUsers/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: AspNetUsers/Create
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create([Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] AspNetUser aspNetUser)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Add(aspNetUser);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(aspNetUser);
//        }

//        // GET: AspNetUsers/Edit/5
//        public async Task<IActionResult> Edit(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
//            if (aspNetUser == null)
//            {
//                return NotFound();
//            }
//            return View(aspNetUser);
//        }

//        // POST: AspNetUsers/Edit/5
//        // To protect from overposting attacks, enable the specific properties you want to bind to.
//        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount")] AspNetUser aspNetUser)
//        {
//            if (id != aspNetUser.Id)
//            {
//                return NotFound();
//            }

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(aspNetUser);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!AspNetUserExists(aspNetUser.Id))
//                    {
//                        return NotFound();
//                    }
//                    else
//                    {
//                        throw;
//                    }
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(aspNetUser);
//        }

//        // GET: AspNetUsers/Delete/5
//        public async Task<IActionResult> Delete(string id)
//        {
//            if (id == null)
//            {
//                return NotFound();
//            }

//            var aspNetUser = await _context.AspNetUsers
//                .FirstOrDefaultAsync(m => m.Id == id);
//            if (aspNetUser == null)
//            {
//                return NotFound();
//            }

//            return View(aspNetUser);
//        }

//        // POST: AspNetUsers/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(string id)
//        {
//            var aspNetUser = await _context.AspNetUsers.FindAsync(id);
//            if (aspNetUser != null)
//            {
//                _context.AspNetUsers.Remove(aspNetUser);
//            }

//            await _context.SaveChangesAsync();
//            return RedirectToAction(nameof(Index));
//        }

//        private bool AspNetUserExists(string id)
//        {
//            return _context.AspNetUsers.Any(e => e.Id == id);
//        }
//    }
//}

using ClothingWebStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AspNetUsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly WebShopContext _context;

    public AspNetUsersController(UserManager<IdentityUser> userManager, WebShopContext context)
    {
        _userManager = userManager;
        _context = context;

    }

    // LIST USERS
    public IActionResult Index()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            return RedirectToAction(nameof(Index));

        // DELETE related cart first
        var cart = await _context.Carts
            .FirstOrDefaultAsync(c => c.UserId == id);

        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        await _userManager.DeleteAsync(user);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(user);
    }
}