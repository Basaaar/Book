using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BulkyBook.Utility;
using Stripe.BillingPortal;
using Stripe.Checkout;

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
				//Burda direk ödeme ekranına yönlendirdik.
				//Iyziconun ödeme ekranına yönlendirmek gerekiyor.
				var domain = "https://localhost:7064/";

                var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl=domain+ $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl=domain+"customer/cart/index",
					LineItems=new List<SessionLineItemOptions> (),					
					Mode="payment",
				};
				foreach(var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price + 100),//$20.50=>2050
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}
						},
						Quantity=item.Count
					};
					options.LineItems.Add(sessionLineItem);
				}
				var service = new Stripe.Checkout.SessionService();
				Stripe.Checkout.Session session=service.Create(options);
				_unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);
			}

			return RedirectToAction(nameof(OrderConfirmation),new { id = ShoppingCartVM.OrderHeader.Id });

		}


		public IActionResult OrderConfirmation(int id)
		{
			//Eğer ödeme ekranında ödeme başarılı olduysa bu ekrana gelecek .
			//Session ıd üzeriden kontrol gerçekleştirerek ödemeini başarı durumunu kontrol edicez.
			//Ödemenin başarılı olması durumunda veri tabanında UpdateStripePAyment güncellemesi yapmıştık.
			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				//this is an order by customer
				var service =new Stripe.Checkout.SessionService();
				Stripe.Checkout.Session session = service.Get(orderHeader.SessionId);
				if (session.PaymentStatus.ToLower() == "paid")//Stripe documentasyonundan baktı payment statuse.
                {

                    _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id,SD.StatusApproved,SD.PaymentStatusApproved);
					_unitOfWork.Save();
                }

			}
			//Shopping chart boşaltmamız gerek.
			List<ShoppingCart> shoppingCarts = _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoopingCart.RemoveRange(shoppingCarts);
			_unitOfWork.Save();
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
			var cartFromDB = _unitOfWork.ShoopingCart.Get(u => u.Id == carId, tracked: true);
			if (cartFromDB.Count <= 1)
			{
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoopingCart.
					GetAll(u => u.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1);
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
			var cartFromDB = _unitOfWork.ShoopingCart.Get(u => u.Id == carId,tracked:true);			
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoopingCart.GetAll(u => u.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1);
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
