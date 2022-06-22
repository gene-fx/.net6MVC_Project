using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;

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

        public IActionResult Detail(int id)
        {
            ShoppingCart shoppingCart = new()
            {
                Product = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == id, includeProperties: "Category,CoverType"),
                Count = 1
            };
            
            return View(shoppingCart);
        }

        //Get
        public IActionResult ProductView(int id)
        {
            Product product = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == id);
            ProductVm productVm = new()
            {
                Product = product,
                CategoryList = _unityOfWork.Category
                .GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
                CoverTypeList = _unityOfWork.CoverType
                .GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                }),
            };

            return View(productVm);
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