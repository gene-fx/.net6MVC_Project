using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    [Area("Admin")]
    public class CoverTypeController : Controller
    {
        private readonly IUnityOfWork _unitOfWork;

        public CoverTypeController(IUnityOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<CoverType> objCoverTypeList = _unitOfWork.CoverType.GetAll();
            return View(objCoverTypeList);
        }

        //GET
        public IActionResult Create()
        {
            return View();
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CoverType obj)
        {
            IEnumerable<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();

            foreach (var coverType in coverTypeList)
            {
                if (obj.Name == coverType.Name)
                {
                    ModelState.AddModelError("name", "These Name is already registred");
                }
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "CoverType created sucessefully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var CoverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

            if (CoverType == null)
            {
                return NotFound();
            }

            return View(CoverType);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CoverType obj)
        {
            IEnumerable<CoverType> coverTypeList = _unitOfWork.CoverType.GetAll();

            foreach (var coverType in coverTypeList)
            {
                if (obj.Name == coverType.Name)
                {
                    ModelState.AddModelError("name", "These Name is already registred");
                }
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.CoverType.Update(obj);
                _unitOfWork.Save();
                TempData["Sucess"] = "CoverType updated sucessefully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        //GET
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var CoverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

            if (CoverType == null)
            {
                return NotFound();
            }

            return View(CoverType);
        }

        //POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePOST(int? id)
        {
            var CoverType = _unitOfWork.CoverType.GetFirstOrDefault(x => x.Id == id);

            if (CoverType == null)
            {
                return NotFound();
            }

            _unitOfWork.CoverType.Remove(CoverType);
            _unitOfWork.Save();
            TempData["Sucess"] = "CoverType deleted sucessefully";
            return RedirectToAction("Index");

        }
    }
}
