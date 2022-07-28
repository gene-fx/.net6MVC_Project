using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnityOfWork _unityOfWork;

        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(IUnityOfWork unityOfWork)
        {
            _unityOfWork = unityOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unityOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            foreach(var cart in ShoppingCartVM.ListCart)
            {
                cart.Price = GetPriceBasedOnQtd(cart.Count, cart.Product.Price, cart.Product.Price50, cart.Product.Price100);

                ShoppingCartVM.CartTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }

        private double GetPriceBasedOnQtd(double qtd, double price, double price50, double price100)
        {
            if (qtd <= 50)
            {
                return price;
            }
            else
            {
                if (qtd >= 100)
                {
                    return price100;
                }
                return price50;
            }
        }

        [HttpPost]
        public JsonResult Plus(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unityOfWork.ShoppingCart.IncrementCount(cart, 1);
            _unityOfWork.Save();
            TempData["Added"] = "Added";
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult Minus(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            if (cart.Count <= 1)
            {
                _unityOfWork.ShoppingCart.Remove(cart);
                TempData["Error"] = "Item removed!"; 
            }
            else
            {
                _unityOfWork.ShoppingCart.DecrementCount(cart, 1);
                TempData["Removed"] = "Removed!";
            }            
            _unityOfWork.Save();
            return Json(new { success = true });
        }

        [HttpPost]
        public JsonResult Remove(int cartId)
        {
            var cart = _unityOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
            _unityOfWork.ShoppingCart.Remove(cart);
            _unityOfWork.Save();
            TempData["Item Removed"] = "Item removed!";
            return Json(new { success = true });
        }
    }
}

