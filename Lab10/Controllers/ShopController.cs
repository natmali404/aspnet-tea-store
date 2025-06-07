using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab10.Data;
using Lab10.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Lab10.Controllers
{
    [Authorize(Policy = "UserPolicy")]
    public class ShopController : Controller
    {

        private readonly AppDbContext _context;
        private readonly ILogger<ShopController> _logger;

        public ShopController(AppDbContext context, ILogger<ShopController> logger)
        {
            _context = context;
            _logger = logger;

        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View();
        }


        //public async Task<IActionResult> AllArticles()
        //{
        //    var articlesWithCategory = await _context.Articles
        //    .Select(article => new
        //    {
        //        Article = article,
        //        CategoryName = _context.Categories
        //            .Where(c => c.Id == article.CategoryId)
        //            .Select(c => c.Name)
        //            .FirstOrDefault()
        //    })
        //    .ToListAsync();

        //    ViewBag.Categories = await _context.Categories.ToListAsync();

        //    TempData["CategoryId"] = 0;

        //    return View("Index", articlesWithCategory);
        //    //return View(articles);
        //}


        //public async Task<IActionResult> ArticlesByCategory(int categoryId)
        //{
        //    var category = await _context.Categories.FindAsync(categoryId);
        //    if (category == null)
        //    {
        //        return NotFound();
        //    }

        //    var articlesWithCategory = await _context.Articles
        //        .Where(a => a.CategoryId == categoryId)
        //        .Select(article => new
        //        {
        //            Article = article,
        //            CategoryName = category.Name 
        //        })
        //        .ToListAsync();

        //    ViewBag.Categories = await _context.Categories.ToListAsync();

        //    TempData["CategoryId"] = categoryId;

        //    return View("Index", articlesWithCategory);
        //}



        public async Task<IActionResult> AllArticles()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View("Index");
            //return View(articles);
        }

        public async Task<IActionResult> ArticlesByCategory(int categoryId)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;

            return View("Index");
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


        //Shopping cart handling
        public JsonResult AddToCart(int id, int categoryId)
        {

            //make the cart unique for each user
            string cartId = HttpContext.Session.GetString("cartId");

            if (string.IsNullOrEmpty(cartId))
            {
                cartId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("cartId", cartId);
            }

            var cart = HttpContext.Request.Cookies["cart"];
            if (string.IsNullOrEmpty(cart))
            {
                cart = "{}";
            }

            //deserialize
            var cartItems = JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            string articleKey = "article" + id;

            if (cartItems.ContainsKey(articleKey))
            {
                cartItems[articleKey]++;
            }
            else
            {
                cartItems.Add(articleKey, 1);
            }

            //serialize back
            HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems), new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
                HttpOnly = true
            });


            ////bonus: return to last viewed category after adding an item to cart
            //if (categoryId == 0)
            //{
            //    return RedirectToAction("AllArticles");
            //}

            //TempData["CategoryId"] = categoryId;

            //return RedirectToAction("ArticlesByCategory", new { categoryId });
            return Json(new { success = true });
        }

    }
}
