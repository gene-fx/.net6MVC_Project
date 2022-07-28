using BulkyBook.DataAccess.Repository.IRepository;
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
        private readonly IUnityOfWork _unityOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnityOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unityOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> objProductControllerList = _unityOfWork.Product.GetAll(includeProperties: "Category,CoverType");
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
            else//if the product already exists this method search for it, to build a ViewModel of it.
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

                if (file != null)//if there is a new file for upload
                {
                    //create a file name for the image that has been upldoaded
                    string fileName = Guid.NewGuid().ToString();
                    //combine the webrootpah with the product images folder
                    var uploads = Path.Combine(wwwRootPath, @"images/products");
                    //extracts the file that has been uploaded extension
                    var extension = Path.GetExtension(file.FileName);

                    //For erase the oder image, in case of adding a new one
                    if (obj.Product.ImageUrl != null) // it checks if the object has a img url
                    {//if its true means that the older image has to be erased 
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
                        _unityOfWork.Product.Add(obj.Product);
                        TempData["Success"] = "Product created sucessefully";
                    }
                    else//if the id isnt null the object is been updated with a new image
                    {
                        var productFromDb = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == obj.Product.Id);

                        //if the url is diferente from the db, the obj.img is been updatade,

                        //so the outcome views has to show the new image
                        if (obj.Product.ImageUrl != productFromDb.ImageUrl)
                        {
                            _unityOfWork.Product.Update(obj.Product);
                            TempData["Success"] = "Product updataded sucessefully";
                        }
                    }
                }
                else//if the update goes without a new image, it get the older one url
                {
                    obj.Product.ImageUrl = Request.Form["oldFile"]; //geting from the html the already existent url to avoid eraese it, and passing to the obj
                    _unityOfWork.Product.Update(obj.Product);//that has been updated
                    TempData["Success"] = "Product updataded sucessefully";
                }
            }

            _unityOfWork.Save();//save then create a productviewmodel to pass the page

            var productFromDbToReturn = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == obj.Product.Id);

            //mounting the product view model with the updated/created obj to pass as a model to the view
            ProductVm productVm = new()
            {
                Product = productFromDbToReturn,
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

            //redirect to another area controller
            //passing the product id to fill the controller parameter
            return (IActionResult)RedirectToAction("ProductView" /*action*/,
                "Home" /*controller*/, new { area = "Customer", productVm.Product.Id } /*area + obj*/);

        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Product obj = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            if (obj == null)
            {
                return NotFound();
            }

            ProductVm productVm = new()
            {
                Product = obj,
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

        //POST
        [HttpPost]
        public IActionResult Delete(int id)
        {


            Product obj = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == id);

            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unityOfWork.Product.Remove(obj);
            _unityOfWork.Save();
            TempData["Success"] = "Product Deleted Successuful";
            return (IActionResult)RedirectToAction("Index");

        }

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


        #region API Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unityOfWork.Product.GetAll(includeProperties: "Category");
            return Json(new { data = productList });
        }

        ////POST
        //[HttpDelete]
        //public IActionResult DeleteApi(int? id)
        //{
        //    var ProductFromDb = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == id);

        //    if (ProductFromDb == null)
        //    {
        //        TempData["Error"] = "";
        //        return Json(new { success = false, message = "Error while deleting" });
        //    }

        //    var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, ProductFromDb.ImageUrl.TrimStart('\\'));
        //    if (System.IO.File.Exists(oldImagePath))
        //    {
        //        System.IO.File.Delete(oldImagePath);
        //    }


        //    _unityOfWork.Product.Remove(ProductFromDb);
        //    _unityOfWork.Save();
        //    TempData["Success"] = "Delete Successful";
        //    return Json(new { success = true, message = "Delete Successful", href = "/Admin/Product/Index" });

        //}

        [HttpPost]
        public JsonResult PostApi(string id, string title, string isbn, string description,
            string author, string listprice, string price, string price50, string price100, 
            string categoryid, string covertypeid, string? oldImgUrl, IFormFile? file)
        {
            if(ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;//get the webrootpath to this var
                
                if(file == null)//update without new img
                {
                    Product updateProduct = new Product()
                    {
                        Id = int.Parse(id),
                        Title = title,
                        ISBN = isbn,
                        Description = description,
                        Author = author,
                        ListPrice = int.Parse(listprice),
                        Price = int.Parse(price),
                        Price50 = int.Parse(price50),
                        Price100 = int.Parse(price100),
                        ImageUrl = oldImgUrl,
                        CategoryId = int.Parse(categoryid),
                        CoverTypeId = int.Parse(covertypeid),
                    };

                    _unityOfWork.Product.Update(updateProduct);
                    TempData["Success"] = "Product updated!";

                }//end of update without new img
                else//update with new img or upload a new product
                {
                    //create a file name for the image that has been upldoaded
                    string fileName = Guid.NewGuid().ToString();

                    //combine the webrootpah with the product images folder
                    var uploads = Path.Combine(wwwRootPath, @"images/products");

                    //extracts the file that has been uploaded extension
                    var extension = Path.GetExtension(file.FileName);

                    //For erase the oder image, in case of adding a new one
                    if (oldImgUrl != null) // it checks if the object has a img url
                    {//if its true means that the older image has to be erased 
                        var oldImagePath = Path.Combine(wwwRootPath, oldImgUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    string newImgUrl = @"\images\products\" + fileName + extension;

                    if (int.Parse(id) == 0)//if the id is null the oject is been created
                    {
                        Product updateProduct = new Product()
                        {
                            Id = int.Parse(id),
                            Title = title,
                            ISBN = isbn,
                            Description = description,
                            Author = author,
                            ListPrice = int.Parse(listprice),
                            Price = int.Parse(price),
                            Price50 = int.Parse(price50),
                            Price100 = int.Parse(price100),
                            ImageUrl = newImgUrl,
                            CategoryId = int.Parse(categoryid),
                            CoverTypeId = int.Parse(covertypeid),
                        };

                        _unityOfWork.Product.Add(updateProduct);
                        TempData["Success"] = "Product created sucessefully";

                    }
                    else//if the id isnt 0 the object is been updated with a new image
                    {
                        var productFromDb = _unityOfWork.Product.GetFirstOrDefault(x => x.Id == int.Parse(id));

                        //if the url is diferente from the db, the obj.img is been updatade,

                        //so the outcome views has to show the new image
                        if (newImgUrl != productFromDb.ImageUrl)
                        {
                            Product updateProduct = new Product()
                            {
                                Id = int.Parse(id),
                                Title = title,
                                ISBN = isbn,
                                Description = description,
                                Author = author,
                                ListPrice = int.Parse(listprice),
                                Price = int.Parse(price),
                                Price50 = int.Parse(price50),
                                Price100 = int.Parse(price100),
                                ImageUrl = newImgUrl,
                                CategoryId = int.Parse(categoryid),
                                CoverTypeId = int.Parse(covertypeid),
                            };
                            _unityOfWork.Product.Update(updateProduct);
                            TempData["Success"] = "Product updataded sucessefully";
                        }
                        else
                        {
                            Product updateProduct = new Product()
                            {
                                Id = int.Parse(id),
                                Title = title,
                                ISBN = isbn,
                                Description = description,
                                Author = author,
                                ListPrice = int.Parse(listprice),
                                Price = int.Parse(price),
                                Price50 = int.Parse(price50),
                                Price100 = int.Parse(price100),
                                ImageUrl = oldImgUrl,
                                CategoryId = int.Parse(categoryid),
                                CoverTypeId = int.Parse(covertypeid),
                            };
                            _unityOfWork.Product.Update(updateProduct);
                            TempData["Success"] = "Product updataded sucessefully";
                        }
                    }
                }//fim else "update with new img or upload a new product"

                _unityOfWork.Save();
                return Json(new { success = true, href = Url.Action("Index","Home", new {Area = "Customer"}) });

            }
            else//if the modelState is not valid
            {
                return Json(new { success = false });
            }
            
        }//fim PostApi

    }
}
#endregion

