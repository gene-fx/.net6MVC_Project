﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            if (id == null || id == 0)
            {
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
            else
            {
                Product product = _unitOfWork.Product.GetFirstOrDefault(x=> x.Id == id);
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
        public IActionResult Upsert(ProductVm obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    //create method
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images/products");
                    var extension = Path.GetExtension(file.FileName);

                    if(obj.Product.ImageUrl != null)
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

                    if(obj.Product.Id == 0)
                    {
                        _unitOfWork.Product.Add(obj.Product);
                        TempData["Sucess"] = "Product created sucessefully";
                    }
                    else
                    {
                        _unitOfWork.Product.Update(obj.Product);
                        TempData["Sucess"] = "Product updataded sucessefully";
                    }
                }
                else
                {
                    obj.Product.ImageUrl = Request.Form["oldFile"];
                    _unitOfWork.Product.Update(obj.Product);
                    TempData["Sucess"] = "Product updataded sucessefully";
                }
            }

            _unitOfWork.Save();
            return (IActionResult)RedirectToAction("Index");
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return NotFound();
            }

            return View(ProductFromDb);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var ProductFromDb = _unitOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (ProductFromDb == null)
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(ProductFromDb);
            _unitOfWork.Save();
            TempData["Sucess"] = "ProductController deleted sucessefully";
            return RedirectToAction("Index");

        }


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }
        #endregion


    }
}