using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // IFormFile kullanmak için
using Microsoft.EntityFrameworkCore;
using System.IO;
using ETicaret.Models; // Modeli kullanabilmek için
using ETicaret.Data; // DbContext'i kullanabilmek içi

namespace ETicaret.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Product
        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            var product = new Product();
            return View(product);
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile image)
        {
            if (string.IsNullOrEmpty(product.Name))
            {
                ViewData["NameError"] = "Product name is required.";
                return View(product);
            }

            if (product.Price <= 0)
            {
                ViewData["PriceError"] = "Product price must be greater than zero.";
                return View(product);
            }

            if (image != null && image.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ViewData["ImageError"] = "Only image files (jpg, jpeg, png) are allowed.";
                    return View(product);
                }

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath); // Dizin yoksa oluştur
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(directoryPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                product.ImageUrl = "/images/" + uniqueFileName; // Veritabanına benzersiz dosya adı kaydediliyor
            }
            else
            {
                ViewData["ImageError"] = "Please upload an image.";
                return View(product);
            }

            _context.Add(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // Diğer CRUD işlemleri
    }
}
