using Lab10.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Lab10.Models;
using System.Security.Claims;

namespace Lab10.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CartController(AppDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // GET: CartController
        [Authorize(Policy = "UserPolicy")]
        public ActionResult Index()
        {
            var cart = HttpContext.Request.Cookies["cart"];
            var cartItems = string.IsNullOrEmpty(cart) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            if (!cartItems.Any())
            {
                return View(new List<CartItemViewModel>());
            }

            //cart id to article id
            var articleIds = cartItems.Keys.Aggregate(new List<int>(), (newId, oldId) =>
            {
                newId.Add(int.Parse(oldId.Replace("article", "")));
                return newId;
            });


            //get articles from database
            var cartArticles = _context.Articles.Where(article => articleIds.Contains(article.Id)).ToList();


            //update cart to contain existing items only
            var updatedCartItems = cartItems
                .Where(kvp => cartArticles.Any(article => article.Id == int.Parse(kvp.Key.Replace("article", ""))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (updatedCartItems.Count != cartItems.Count)
            {
                Response.Cookies.Append("cart", JsonSerializer.Serialize(updatedCartItems), new CookieOptions { Expires = DateTime.UtcNow.AddDays(7) });
            }

            //map to a viewmodel
            var cartItemViewModels = cartArticles.Select(article => new CartItemViewModel
            {
                Article = article,
                Quantity = cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0,
                TotalPrice = article.Price * (cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0)
            }).ToList();

            return View(cartItemViewModels);
        }



        public ActionResult UpdateCart(int articleId, int quantity)
        {
            var cart = HttpContext.Request.Cookies["cart"];
            var cartItems = string.IsNullOrEmpty(cart) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            cartItems["article" + articleId] = quantity;

            HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems));

            return RedirectToAction("Index");
        }



        public ActionResult RemoveFromCart(int articleId)
        {
            var cart = HttpContext.Request.Cookies["cart"];
            var cartItems = string.IsNullOrEmpty(cart) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            cartItems.Remove("article" + articleId);
            HttpContext.Response.Cookies.Append("cart", JsonSerializer.Serialize(cartItems));

            return RedirectToAction("Index");
        }

        [Authorize]

        public ActionResult Checkout()
        {
            var cart = HttpContext.Request.Cookies["cart"];
            var cartItems = string.IsNullOrEmpty(cart) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            if (!cartItems.Any())
            {
                return View(new List<CartItemViewModel>());
            }

            //cart id to article id
            var articleIds = cartItems.Keys.Aggregate(new List<int>(), (newId, oldId) =>
            {
                newId.Add(int.Parse(oldId.Replace("article", "")));
                return newId;
            });


            //get articles from database
            var cartArticles = _context.Articles.Where(article => articleIds.Contains(article.Id)).ToList();


            //update cart to contain existing items only
            var updatedCartItems = cartItems
                .Where(kvp => cartArticles.Any(article => article.Id == int.Parse(kvp.Key.Replace("article", ""))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (updatedCartItems.Count != cartItems.Count)
            {
                Response.Cookies.Append("cart", JsonSerializer.Serialize(updatedCartItems), new CookieOptions { Expires = DateTime.UtcNow.AddDays(7) });
            }

            //map to a viewmodel
            var cartItemViewModels = cartArticles.Select(article => new CartItemViewModel
            {
                Article = article,
                Quantity = cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0,
                TotalPrice = article.Price * (cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0)
            }).ToList();

            return View(cartItemViewModels);
        }


        private List<CartItemViewModel> GetCartItems()
        {
            var cart = HttpContext.Request.Cookies["cart"];
            var cartItems = string.IsNullOrEmpty(cart) ? new Dictionary<string, int>() : JsonSerializer.Deserialize<Dictionary<string, int>>(cart);

            if (!cartItems.Any())
            {
                return new List<CartItemViewModel>();
            }

            var articleIds = cartItems.Keys.Select(key => int.Parse(key.Replace("article", ""))).ToList();
            var cartArticles = _context.Articles.Where(article => articleIds.Contains(article.Id)).ToList();

            return cartArticles.Select(article => new CartItemViewModel
            {
                Article = article,
                Quantity = cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0,
                TotalPrice = article.Price * (cartItems.ContainsKey("article" + article.Id) ? cartItems["article" + article.Id] : 0)
            }).ToList();
        }

        private void ClearCart()
        {
            HttpContext.Response.Cookies.Delete("cart");
        }


        [HttpPost]
        [Authorize]
        public IActionResult ConfirmOrder(OrderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //addr
            var newAddress = new Address
            {
                UserId = userId,
                Name = model.Name,
                Surname = model.Surname,
                Street = model.Street,
                PostalCode = model.PostalCode,
                City = model.City
            };

            _context.Add(newAddress);
            _context.SaveChanges();

            //order
            var order = new Order
            {
                UserId = userId,
                AddressId = newAddress.Id,
                PaymentMethod = model.PaymentMethod
            };

            _context.Add(order);
            _context.SaveChanges();


            var cartItems = GetCartItems();

            foreach (var cartItem in cartItems)
            {
                var article = _context.Articles.FirstOrDefault(a => a.Id == cartItem.Article.Id);

                if (article != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ArticleId = article.Id,
                        Quantity = cartItem.Quantity,
                        Price = article.Price,
                        TotalPrice = article.Price * cartItem.Quantity
                    };

                    _context.Add(orderItem);
                }
            }

            _context.SaveChanges();


            ClearCart();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }


        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Set<Order>().FirstOrDefault(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // Join na tabeli OrderItem i Article na podstawie ArticleId
            var orderItems = _context.Set<OrderItem>()
                .Where(oi => oi.OrderId == orderId)
                .Join(_context.Set<Article>(),
                      oi => oi.ArticleId,
                      a => a.Id,
                      (oi, a) => new { OrderItem = oi, Article = a })
                .ToList();

            var viewModel = new
            {
                Order = order,
                OrderItems = orderItems.Select(x => new
                {
                    x.OrderItem,
                    x.Article
                }).ToList()
            };

            return View(viewModel);
        }





    }
}
