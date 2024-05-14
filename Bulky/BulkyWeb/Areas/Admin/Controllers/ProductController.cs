using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Product> products = unitOfWork.Product.GetAll().ToList();

            return View(products);
        }
        public IActionResult Upsert(int? id) //Update and Insert
        {

            //ViewBag.CategoryList = CategoryList;    
            //ViewData["CategoryList"] = CategoryList;
            ProductVM productVM = new()
            {
                CategoryList = unitOfWork.Category.GetAll().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()

                }),
                Product = new Product()
            };
            if(id==null || id==0)
            {
                //Create
                return View(productVM);
            }
            else//Update
            {
                productVM.Product = unitOfWork.Product.Get(u => u.Id == id);
            }
            return View(productVM);//Eğer bu obje gönderilmez ise yine .net view kısmından yeni bir obje oluşturabilir.
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {



            if (ModelState.IsValid)
            {
                unitOfWork.Product.Add(obj.Product);
                unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {

                obj.CategoryList = unitOfWork.Category.GetAll().ToList().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()

                });
                   
                
                return View(obj);

            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }
            Product? prodcutFromDb = unitOfWork.Product.Get(u => u.Id == id);
            if (prodcutFromDb == null)
            {
                return NotFound();
            }
            return View(prodcutFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {


            if (ModelState.IsValid)
            {
                unitOfWork.Product.Update(product);
                unitOfWork.Save();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index", "Product");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            if (id == null && id == 0)
            {
                return NotFound();
            }
            Product? prodcutFromDb = unitOfWork.Product.Get(u => u.Id == id);
            if (prodcutFromDb == null)
            {
                return NotFound();
            }
            return View(prodcutFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {

            Product? prodcutFromDb = unitOfWork.Product.Get(u => u.Id == id);
            if (prodcutFromDb == null)
                return NotFound();

            unitOfWork.Product.Remove(prodcutFromDb);
            unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index", "Product");

        }
    }
}
