using BookZone.Attributes;
using BookZone.DataAccess.Repository.IRepository;
using BookZone.Models;
using BookZone.Models.ViewModels;
using BookZone.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookZone.Areas.Customer.Controllers
{
    [Area("Customer")]
    [CustomAuthorize]
    public class ShoppingCartController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM _shoppingCartVM { get; set; }
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new ()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new OrderHeader()
                };
            }
            else
            {
                // Handle the case where userId is not a valid int
                return BadRequest("Invalid user ID");
            }

            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
               cart.Price = calculateOrderTotal(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(_shoppingCartVM);
        }




        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new OrderHeader()
                };
            }
            else
            {
                // Handle the case where userId is not a valid int
                return BadRequest("Invalid user ID");
            }

            _shoppingCartVM.OrderHeader.User = _unitOfWork.Users.Get(u => u.Id == parsedUserId);




            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = calculateOrderTotal(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(_shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {
                _shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product");
            }
            else
            {
                // Handle the case where userId is not a valid int
                return BadRequest("Invalid user ID");
            }

            _shoppingCartVM.OrderHeader.UserId = parsedUserId;
            _shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;

            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = calculateOrderTotal(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            _shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            _shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

            _unitOfWork.OrderHeader.Add(_shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = _shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };

                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            return RedirectToAction("Payment", "Payment");
        }

        


        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count > 1) 
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
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
