using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Bulky.Book.DataAccess.Repository;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            this.unitOfWork = unitOfWork;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = unitOfWork.Product.GetAll(includeProperties:"Category").ToList();

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
                productVM.Product = unitOfWork.Product.Get(u => u.Id == id,includeProperties:"ProductImages");
            }
            return View(productVM);//Eğer bu obje gönderilmez ise yine .net view kısmından yeni bir obje oluşturabilir.
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM obj,List<IFormFile> files)
        {

            if (ModelState.IsValid)
            {
                if (obj.Product.Id == 0)
                {
                    unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    unitOfWork.Product.Update(obj.Product);
                }

                unitOfWork.Save();

                string wwwRootPath=webHostEnvironment.WebRootPath;
                if(files!=null)
                {


                    foreach(IFormFile file in files)
                    {

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"images\products\product-"+obj.Product.Id);
                        string finalPAth = Path.Combine(wwwRootPath, productPath);
                        if(!Directory.Exists(finalPAth))
                            Directory.CreateDirectory(finalPAth);
                        using (FileStream fileStram = new FileStream(Path.Combine(finalPAth, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStram);
                        }

                        ProductImage productImage = new ProductImage()
                        {

                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = obj.Product.Id,

                        };
                        if (obj.Product.ProductImages == null)
                            obj.Product.ProductImages = new List<ProductImage>();

                        obj.Product.ProductImages.Add(productImage);
                      
                    }
                    unitOfWork.Product.Update(obj.Product);
                    unitOfWork.Save();

                    
                }
               
                TempData["success"] = "Product created/updated successfully";
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
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null && id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? prodcutFromDb = unitOfWork.Product.Get(u => u.Id == id);
        //    if (prodcutFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(prodcutFromDb);
        //}
        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{

        //    Product? prodcutFromDb = unitOfWork.Product.Get(u => u.Id == id);
        //    if (prodcutFromDb == null)
        //        return NotFound();

        //    unitOfWork.Product.Remove(prodcutFromDb);
        //    unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";
        //    return RedirectToAction("Index", "Product");

        //}


        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath =
                                   Path.Combine(webHostEnvironment.WebRootPath,
                                   imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                unitOfWork.ProductImage.Remove(imageToBeDeleted);
                unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }




        #region API CALLS
        [HttpGet]
        public IActionResult GetAll() {

            List<Product> objProductList = unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new {data= objProductList });


        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var productToBeDeleted = unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            unitOfWork.Product.Remove(productToBeDeleted);
            unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });

        }

        #endregion
    }
}
