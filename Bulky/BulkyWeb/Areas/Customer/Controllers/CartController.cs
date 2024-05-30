using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BulkyBook.Utility;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]//Automatically populated this value when post action 
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			this._unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			var claimsIdentiy = (ClaimsIdentity)User.Identity;//Default property
			var userID = claimsIdentiy.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == userID, includeProperties: "Product"),
				OrderHeader = new(),
			};
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);
		}
		public IActionResult Summary()
		{
			var claimsIdentiy = (ClaimsIdentity)User.Identity;//Default property
			var userID = claimsIdentiy.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == userID, includeProperties: "Product"),
				OrderHeader = new(),
			};
			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userID);


			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
			



			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVM);

		}
		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentiy = (ClaimsIdentity)User.Identity;//Default property
			var userID = claimsIdentiy.FindFirst(ClaimTypes.NameIdentifier).Value;
			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == userID, includeProperties: "Product");


			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userID;


			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userID);

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//its a regular customer account and we need to capture payment
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

			}
			else
			{
				//its a company user
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
			_unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//its a regular customer account and we need to capture payment
				//stripe logic 

			}

			return RedirectToAction(nameof(OrderConfirmation),new { id = ShoppingCartVM.OrderHeader.Id });

		}


		public IActionResult OrderConfirmation(int id)
		{
			return View(id);
		}

		public IActionResult Plus(int carId)
		{
			var cartFromDB = _unitOfWork.ShoopingCart.Get(u => u.Id == carId);
			cartFromDB.Count += 1;
			_unitOfWork.ShoopingCart.Update(cartFromDB);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Minus(int carId)
		{
			var cartFromDB = _unitOfWork.ShoopingCart.Get(u => u.Id == carId);
			if (cartFromDB.Count <= 1)
			{
				_unitOfWork.ShoopingCart.Remove(cartFromDB);
			}
			else
			{
				cartFromDB.Count -= 1;
				_unitOfWork.ShoopingCart.Update(cartFromDB);
			}


			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Remove(int carId)
		{
			var cartFromDB = _unitOfWork.ShoopingCart.Get(u => u.Id == carId);
			_unitOfWork.ShoopingCart.Remove(cartFromDB);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}



		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
				return shoppingCart.Product.Price;
			else
			{
				if (shoppingCart.Count <= 100)
					return shoppingCart.Product.Price50;
				else
					return shoppingCart.Product.Price100;
			}
		}
	}
}
