﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnityOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnityOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductControllerList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return (IActionResult)View(objProductControllerList);
        }

        //GET
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0) //test if the product already exists, if not it constructs the category and covertype lists
            {                          //to display as a dropdown menu 
                ProductVm productVm = new()
                {
                    Product = new(),
                    CategoryList = _unitOfWork.Category
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                };
                return View(productVm);
            }
            else//if the product already exists this method search for it, to build a ViewModel of it.
            {
                Product product = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);
                ProductVm productVm = new()
                {
                    Product = product,
                    CategoryList = _unitOfWork.Category
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    CoverTypeList = _unitOfWork.CoverType
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                };
                return View(productVm);
            }
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = int.MaxValue)]
        public IActionResult Upsert(ProductVm obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;//get the webrootpath to this var
                if (file != null)//if there is a new file
                {
                    //create method
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images/products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.Product.ImageUrl != null) //For erase the oder image, in case of adding a new one
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\products\" + fileName + extension;

                    if (obj.Product.Id == 0)//if the id is null the oject is been created
                    {
                        _unitOfWork.Product.Add(obj.Product);
                        TempData["Sucess"] = "Product created sucessefully";
                    }
                    else//if the id isnt null the object is been updated
                    {
                        var productFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == obj.Product.Id);

                        //if the url is diferente from the db, the obj.img is been updatade,
                        //so the outcome views has to show the new image
                        if (obj.Product.ImageUrl != productFromDb.ImageUrl)
                        {
                            _unitOfWork.Product.Update(obj.Product);
                            TempData["Sucess"] = "Product updataded sucessefully";
                        }
                    }
                }
                else//if the update goes without a new image, get the older one and return to table view
                {
                    obj.Product.ImageUrl = Request.Form["oldFile"];
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["Sucess"] = "Product updataded sucessefully";
                }
            }

            _unitOfWork.Save();//save then create a productviewmodel to pass the page

            var productFromDbToReturn = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == obj.Product.Id);

            ProductVm productVm = new()
            {
                Product = productFromDbToReturn,
                CategoryList = _unitOfWork.Category
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                CoverTypeList = _unitOfWork.CoverType
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
            };
            return (IActionResult)RedirectToAction("Index", "Home", new { Area = "Customer" });
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if(id == 0)
            {
                return NotFound();
            }

            Product obj = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            ProductVm productVm = new()
            {
                Product = obj,
                CategoryList = _unitOfWork.Category
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                CoverTypeList = _unitOfWork.CoverType
                    .GetAll().Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
            };

            return View(productVm);
        }

        //POST
        [HttpPost]
        public IActionResult Delete(int id)
        {
            

            Product obj = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["Sucess"] = "Product Deleted Successuful";
            return (IActionResult)RedirectToAction("Index");

        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }

        //POST
        [HttpDelete]
        public IActionResult DeleteApi(int? id)
        {
            var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (ProductFromDb == null)
            {
                TempData["Error"] = "";
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, ProductFromDb.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }


            _unitOfWork.Product.Remove(ProductFromDb);
            _unitOfWork.Save();
            TempData["Sucess"] = "Delete Successful";
            return Json(new { success = true, message = "Delete Successful", href = "/Admin/Product/Index" });

        }

        [HttpPost]
        public JsonResult PostApi(string id, string tile, string isbn,
            string description, string author, string listPrice,
            string price, string price50, string price100, string imgUrl, IFormFile? img)
        {
            if (id != null)
            {
                TempData["success"] = "success";
                return Json(new { success = true, message = "Update Successful", href = "/Admin/Product/Index" });
            }
            else
            {
                TempData["error"] = "error";
                return Json(new { success = false, message = "Update Unsuccessful" });
            }
        }
        #endregion


    }
}
