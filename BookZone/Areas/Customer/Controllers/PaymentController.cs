﻿// PaymentController.cs
using BookZone.Attributes.PaymentStrategy;
using BookZone.Models.ViewModels;
using BookZone.Models;
using BookZone.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using BookZone.DataAccess.Repository.IRepository;
using BookZone.Attributes;

namespace BookZone.Areas.Customer.Controllers
{
    [Area("Customer")]
    [CustomAuthorize]
    public class PaymentController : Controller
    {
        private readonly PaymentContext _paymentContext;
        private readonly IUnitOfWork _unitOfWork;
        private ShoppingCartVM _shoppingCartVM;

        public PaymentController(IUnitOfWork unitOfWork)
        {
            _paymentContext = new PaymentContext();
            _unitOfWork = unitOfWork;
        }


        // GET: Payment
        public IActionResult Payment()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                foreach (var cart in _shoppingCartVM.ShoppingCartList)
                {
                    cart.Price = CalculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());

                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


        // POST: Payment according to the selected method it will redirect to the respective payment method
        [HttpPost]
        public IActionResult Payment(string paymentMethod)
        {
            HttpContext.Session.SetString("PaymentMethod", paymentMethod);

            switch (paymentMethod)
            {
                case "CreditCard":
                    return RedirectToAction("CreditCardPayment");
                case "PayPal":
                    return RedirectToAction("PaypalPayment");
                case "BankTransfer":
                    return RedirectToAction("BankTransferPayment");
                default:
                    return BadRequest("Invalid payment method.");
            }
        }


        // GET: CreditCardPayment
        public IActionResult CreditCardPayment()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                foreach (var cart in _shoppingCartVM.ShoppingCartList)
                {
                    cart.Price = CalculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());//  Set order total in session

                _paymentContext.SetPaymentStrategy(new CreditCardPayment());// Set payment strategy
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


        // GET: BankTransferPayment
        public IActionResult BankTransferPayment()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                foreach (var cart in _shoppingCartVM.ShoppingCartList)
                {
                    cart.Price = CalculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());//  Set order total in session

                _paymentContext.SetPaymentStrategy(new BankTransferPayment());// Set payment strategy
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


        // GET: PaypalPayment
        public IActionResult PaypalPayment()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new()
                };

                foreach (var cart in _shoppingCartVM.ShoppingCartList)
                {
                    cart.Price = CalculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());//  Set order total in session



                _paymentContext.SetPaymentStrategy(new PayPalPayment());//  Set payment strategy
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


        // POST: PaymentSuccess
        [HttpPost]
        public IActionResult PaymentSuccess()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                var orderHeader = _unitOfWork.OrderHeader
                    .Get(u => u.UserId == parsedUserId && u.PaymentStatus == SD.PaymentStatusPending);

                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId,
                        includeProperties: "Product"),
                    OrderHeader = orderHeader
                };

                var orderTotal = HttpContext.Session.GetString("OrderTotal");
                if (!string.IsNullOrEmpty(orderTotal))
                {
                    _shoppingCartVM.OrderHeader.OrderTotal = Convert.ToDouble(orderTotal);
                }

                _shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                _shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                _unitOfWork.OrderHeader.Update(_shoppingCartVM.OrderHeader);
                _unitOfWork.Save();

                List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                    .GetAll(u => u.ApplicationUserId == parsedUserId).ToList();
                _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
                _unitOfWork.Save();

                decimal receivedAmount = (decimal)_shoppingCartVM.OrderHeader.OrderTotal;

                var paymentMethod = HttpContext.Session.GetString("PaymentMethod");

                // Set payment strategy
                switch (paymentMethod)
                {
                    case "CreditCard":
                        _paymentContext.SetPaymentStrategy(new CreditCardPayment());
                        break;
                    case "PayPal":
                        _paymentContext.SetPaymentStrategy(new PayPalPayment());
                        break;
                    case "BankTransfer":
                        _paymentContext.SetPaymentStrategy(new BankTransferPayment());
                        break;
                    default:
                        return BadRequest("Invalid payment method.");
                }

                _paymentContext.ExecutePayment(receivedAmount, receivedAmount);// Execute payment
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


        // calculates total order for the given shopping cart
        private double CalculateOrderTotal(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count > 50 && shoppingCart.Count <= 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
