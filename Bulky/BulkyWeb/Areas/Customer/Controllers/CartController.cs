using Bulky.Book.DataAccess.Repository.IRepository;
using Bulky.Book.Models.ViewModels;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController:Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentiy = (ClaimsIdentity)User.Identity;//Default property
            var userID = claimsIdentiy.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new ()
            {
                ShoppingCartList = _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == userID,includeProperties:"Product")
            };
            foreach(var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price=GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }
            return View(ShoppingCartVM);
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;
            else
            {
                if(shoppingCart.Count<=100)
                    return shoppingCart.Product.Price50;
                else
                    return shoppingCart.Product.Price100;
            }
        }
    }
}
