using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab10.Data;
using Lab10.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Lab10.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ArticlesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public ArticlesController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            //get category names
            var articlesWithCategories = await _context.Articles
            .Select(article => new
            {
                Article = article,
                CategoryName = _context.Categories
                    .Where(c => c.Id == article.CategoryId)
                    .Select(c => c.Name)
                    .FirstOrDefault()
            })
            .OrderBy(ac => ac.Article.Id)
            .ToListAsync();

            return View(articlesWithCategories);
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            //this gets the category name
            var categoryName = await _context.Categories
            .Where(c => c.Id == article.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
            ViewData["CategoryName"] = categoryName;

            return View(article);
        }

        // GET: Articles/Create
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Price,CategoryId")] Article article, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    _logger.LogInformation("FILE DETECTED");
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/upload");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    article.ImagePath = "/images/upload/" + uniqueFileName;
                }
                else
                {
                    _logger.LogInformation("NO FILE DETECTED...");
                    article.ImagePath = null;
                }
                bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == article.CategoryId);
                if (!categoryExists)
                {
                    article.CategoryId = null;
                }

                _context.Add(article);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError("ModelState error: {ErrorMessage}", error.ErrorMessage);
                }
            }
            return View(article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,ImagePath,CategoryId")] Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            if (ModelState.IsValid)
            {
                try
                {
                    // Pobranie istniejącego artykułu z bazy danych
                    var existingArticle = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    if (existingArticle == null)
                    {
                        return NotFound();
                    }

                    // Zachowanie istniejącej ścieżki do obrazu
                    article.ImagePath = existingArticle.ImagePath;

                    bool categoryExists = await _context.Categories.AnyAsync(c => c.Id == article.CategoryId);
                    if (!categoryExists)
                    {
                        article.CategoryId = null;
                    }

                    // Aktualizacja artykułu w bazie danych
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.Id))
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

            return View(article);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                if (!string.IsNullOrEmpty(article.ImagePath) && article.ImagePath != "/images/content/default.jpg")
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", article.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

        

    }
}
