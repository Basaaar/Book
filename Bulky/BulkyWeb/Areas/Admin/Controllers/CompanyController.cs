using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
       
        public CompanyController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            
        }

        public IActionResult Index()
        {
            List<Company> companys = unitOfWork.Company.GetAll().ToList();

            return View(companys);
        }
        public IActionResult Upsert(int? id) //Update and Insert
        {

            //ViewBag.CategoryList = CategoryList;    
            //ViewData["CategoryList"] = CategoryList;
           
            if (id == null || id == 0)
            {
                //Create
                return View(new Company());
            }
            else//Update
            {
                Company company = unitOfWork.Company.Get(u => u.Id == id);
                return View(company);
            }
            //Eğer bu obje gönderilmez ise yine .net view kısmından yeni bir obje oluşturabilir.
        }
        [HttpPost]
        public IActionResult Upsert(Company obj)
        {

            if (ModelState.IsValid)
            {
                
              
                if (obj.Id == 0)
                {
                    unitOfWork.Company.Add(obj);
                }
                else
                {
                    unitOfWork.Company.Update(obj);
                }

                unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index", "Company");
            }
            else
            {

                return View(obj);

            }

        }
        public IActionResult Edit(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }
            Company? prodcutFromDb = unitOfWork.Company.Get(u => u.Id == id);
            if (prodcutFromDb == null)
            {
                return NotFound();
            }
            return View(prodcutFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Company company)
        {


            if (ModelState.IsValid)
            {
                unitOfWork.Company.Update(company);
                unitOfWork.Save();
                TempData["success"] = "Company updated successfully";
                return RedirectToAction("Index", "Company");
            }
            return View();
        }
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null && id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? prodcutFromDb = unitOfWork.Company.Get(u => u.Id == id);
        //    if (prodcutFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(prodcutFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{

        //    Company? prodcutFromDb = unitOfWork.Company.Get(u => u.Id == id);
        //    if (prodcutFromDb == null)
        //        return NotFound();

        //    unitOfWork.Company.Remove(prodcutFromDb);
        //    unitOfWork.Save();
        //    TempData["success"] = "Company deleted successfully";
        //    return RedirectToAction("Index", "Company");

        //}
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {

            List<Company> objCompanyList = unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });


        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            Company? company = unitOfWork.Company.Get(u => u.Id == id);
            if (company == null)
            {
                return Json(new { success = true, message = "Error while deleting" });

            }
           
            unitOfWork.Company.Remove(company);
            unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
