using Bulky.Book.DataAccess.Repository.IRepository;

using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController:Controller
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
        public IActionResult Create()
        {
            IEnumerable<SelectListItem> CategoryList = unitOfWork.Category.GetAll().ToList().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()

            });
            ViewBag.CategoryList = CategoryList;    
            return View(new Product());//Eğer bu obje gönderilmez ise yine .net view kısmından yeni bir obje oluşturabilir.
        }
        [HttpPost]
        public IActionResult Create(Product product)
        {
            
            

            if (ModelState.IsValid)
            {
                unitOfWork.Product.Add(product);
                unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index", "Product");
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
