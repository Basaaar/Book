
using Bulky.Book.DataAccess.Repository;
using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //Return some view inside the View folder.İf no name inside View() method,it return same name with method name.
            //View-->Home-->Index.cshtml
        
            IEnumerable<Product> prdouctList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(prdouctList);
        }
        public IActionResult Details(int productId)
        {
            //Return some view inside the View folder.İf no name inside View() method,it return same name with method name.
            //View-->Home-->Index.cshtml
            ShoppingCart shoppingCart = new()
            {
                Count=1,
                ProductId=productId,
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
            };
        
        
            return View(shoppingCart);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //Return some view inside the View folder.İf no name inside View() method,it return same name with method name.
            //View-->Home-->Index.cshtml

            var claimsIdentiy = (ClaimsIdentity)User.Identity;//Default property
            var userID = claimsIdentiy.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userID;

            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userID &&
            u.ProductId == shoppingCart.ProductId);
            if(cartFromDB!=null)
            {
                cartFromDB.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDB);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userID).Count());
            }
            TempData["success"] = "Cart updated successfuly";

            _unitOfWork.Save();
           

            return RedirectToAction(nameof(Index));
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