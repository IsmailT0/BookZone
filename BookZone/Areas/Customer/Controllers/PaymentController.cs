// PaymentController.cs
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
                    cart.Price = calculateOrderTotal(cart);
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


        [HttpPost]
        public IActionResult Payment(string paymentMethod)
        {
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
                    cart.Price = calculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());

                _paymentContext.SetPaymentStrategy(new CreditCardPayment());
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


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
                    cart.Price = calculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());
                _paymentContext.SetPaymentStrategy(new BankTransferPayment());
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }
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
                    cart.Price = calculateOrderTotal(cart);
                    _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
                }

                HttpContext.Session.SetString("OrderTotal", _shoppingCartVM.OrderHeader.OrderTotal.ToString());

                _paymentContext.SetPaymentStrategy(new PayPalPayment());
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }


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

                decimal recievedAmount = (decimal)_shoppingCartVM.OrderHeader.OrderTotal;
                _paymentContext.ProcessPayment(recievedAmount,recievedAmount);
                return View(_shoppingCartVM);
            }
            else
            {
                return BadRequest("Invalid user ID");
            }
        }

        private double calculateOrderTotal(ShoppingCart shoppingCart)
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
