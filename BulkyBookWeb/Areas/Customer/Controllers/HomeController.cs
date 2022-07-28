using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnityOfWork _unityOfWork;

        public HomeController(ILogger<HomeController> logger, IUnityOfWork unityOfWork)
        {
            _logger = logger;
            _unityOfWork = unityOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unityOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(products);
        }

        public IActionResult Detail(int productId)
        {
            ShoppingCart objCart = new()
            {
                ProductId = productId,
                Product = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == productId, includeProperties: "Category,CoverType"),
                Count = 1
            };
            
            return View(objCart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Detail(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = claim.Value;

            ShoppingCart cartFromDb = _unityOfWork.ShoppingCart.GetFirstOrDefault(u => u.ApplicationUserId == claim.Value &&
                u.ProductId == shoppingCart.ProductId);

            if(cartFromDb != null)
            {
                _unityOfWork.ShoppingCart.IncrementCount(cartFromDb, shoppingCart.Count);
                TempData["Success"] = "Cart updated!";
            }
            else
            {
                _unityOfWork.ShoppingCart.Add(shoppingCart);
                TempData["Success"] = "Product sucessefully added to cart!";
            }

            _unityOfWork.Save();

            
            return RedirectToAction(nameof(Index)); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}