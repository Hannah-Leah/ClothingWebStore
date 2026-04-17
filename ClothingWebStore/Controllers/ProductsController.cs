using ClothingWebStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ClothingWebStore.Controllers
{

    
    public class ProductsController : Controller
    {
        private readonly WebShopContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProductsController(WebShopContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var webShopContext = _context.Products.Include(p => p.Category).Include(p => p.Manufacturer).Include(p => p.ProductImages); ;
            return View(await webShopContext.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                 .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name");
            return View();
        }

        [Authorize(Roles = "Admin")]
        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]


       /*
        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,ItemNumber,Price,Stock,IsArchived,CategoryId,ManufacturerId")] Product product,
    IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                // HANDLE IMAGE UPLOAD
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save path to DB
                    var productImage = new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = "/images/" + fileName
                    };

                    _context.ProductImages.Add(productImage);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name", product.ManufacturerId);

            return View(product);
        }
       */


        public async Task<IActionResult> Create([Bind("ProductId,Name,Description,ItemNumber,Price,Stock,IsArchived,CategoryId,ManufacturerId")] Product product,
    List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                // HANDLE IMAGE UPLOAD
                if (imageFiles != null && imageFiles.Any())
                {
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                            var filePath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot/images",
                                fileName
                            );

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }

                            _context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = "/images/" + fileName
                            });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name", product.ManufacturerId);

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name", product.ManufacturerId);
            return View(product);
        }

       [Authorize(Roles = "Admin")]
        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        /*
         
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,ItemNumber,Price,Stock,IsArchived,CategoryId,ManufacturerId")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name", product.ManufacturerId);
            return View(product);
        }
         
          */
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,ItemNumber,Price,Stock,IsArchived,CategoryId,ManufacturerId")] Product product, List<IFormFile> imageFiles)
        {

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
                ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers, "ManufacturerId", "Name", product.ManufacturerId);
                return View(product);
            }

            try
            {
              
                var existingProduct = await _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (existingProduct == null)
                {
                    return NotFound();
                }

                
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.ItemNumber = product.ItemNumber;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.IsArchived = product.IsArchived;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ManufacturerId = product.ManufacturerId;

                // Add new uploaded images
                if (imageFiles != null && imageFiles.Any())
                {
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                            var filePath = Path.Combine(
                                Directory.GetCurrentDirectory(),
                                "wwwroot/images",
                                fileName
                            );

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }

                            existingProduct.ProductImages.Add(new ProductImage
                            {
                                ProductId = existingProduct.ProductId,
                                ImageUrl = "/images/" + fileName
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(product.ProductId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }



        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Manufacturer)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = "Admin")]

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
