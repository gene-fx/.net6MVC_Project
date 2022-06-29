using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class CompanyController : Controller
    {
        private readonly IUnityOfWork _unitOfWork;

        public CompanyController(IUnityOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Company> objComapnyControllerList = _unitOfWork.Company.GetAll();
            return (IActionResult)View(objComapnyControllerList);
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)//if the id is null it generates a new Company obj with null instances to fill the page
            {  
                Company company = new();

                return View(company);
            }
            else//else it seach for the company id into the DB and return to populate the page with the info
            {
                Company company = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);

                return View(company);
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public IActionResult Upsert(Company obj)
        {
            if (ModelState.IsValid)
            {
                if (obj.Id == 0)//if the id is null the oject is been created
                {
                    _unitOfWork.Company.Add(obj);
                    TempData["Sucess"] = "Company created sucessefully";
                }
                else//if the id isnt null the object is been updated
                {
                    var companyFromDb = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == obj.Id);

                    _unitOfWork.Company.Update(obj);
                    TempData["Sucess"] = "Company updataded sucessefully";
                }

                _unitOfWork.Save();
            }
            return View(obj);
        }

        //POST
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Company obj = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);

            _unitOfWork.Company.Remove(obj);
            _unitOfWork.Save();
            TempData["Sucess"] = "Product Deleted Successuful";
            return (IActionResult)RedirectToAction("Index");
        }


        #region API Calls
        //For now it wont call APIs

        //[HttpGet]
        //public IActionResult GetAll()
        //{
        //    var productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
        //    return Json(new { data = productList });
        //}

        //POST
        [HttpDelete]
        public IActionResult DeleteApi(int? id)
        {
            var companyFromDb = _unitOfWork.Company.GetFirstOrDefault(x => x.Id == id);

            if (companyFromDb == null)
            {
                TempData["Error"] = "";
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.Company.Remove(companyFromDb);
            _unitOfWork.Save();
            TempData["Sucess"] = "Delete Successful";
            return Json(new { success = true, message = "Delete Successful", href = "/Admin/Product/Index" });

        }

        //[HttpPost]
        //public JsonResult PostApi(string id, string tile, string isbn,
        //    string description, string author, string listPrice,
        //    string price, string price50, string price100, string imgUrl, IFormFile? img)
        //{
        //    if (id != null)
        //    {
        //        TempData["success"] = "success";
        //        return Json(new { success = true, message = "Update Successful", href = "/Admin/Product/Index" });
        //    }
        //    else
        //    {
        //        TempData["error"] = "error";
        //        return Json(new { success = false, message = "Update Unsuccessful" });
        //    }
        //}
        #endregion


    }
}
