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


        // GET: ShoppingCartController
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userId, out int parsedUserId))
            {

                // Get all shopping cart items for the current user
                _shoppingCartVM = new ()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new OrderHeader()
                };


            }
            else
            {
                
                return BadRequest("Invalid user ID");
            }


            // Get the user details
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

                //create the shopping cart view model for the current user
                _shoppingCartVM = new()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderHeader = new OrderHeader()
                };
            }
            else
            {
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


        // POST: ShoppingCartController/Summary
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

            // Create the order header
            _shoppingCartVM.OrderHeader.UserId = parsedUserId;
            _shoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;


            // Calculate the order total
            foreach (var cart in _shoppingCartVM.ShoppingCartList)
            {
                cart.Price = calculateOrderTotal(cart);
                _shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }


            // Save the order header to the database
            _shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            _shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;


            // Add the order header to the database
            _unitOfWork.OrderHeader.Add(_shoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            // Add the order details to the database
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


            // go to the payment page
            return RedirectToAction("Payment", "Payment");
        }



        // Increases the count of a product in the shopping cart
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cart.Count += 1;
            _unitOfWork.ShoppingCart.Update(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        // Decreases the count of a product in the shopping cart
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
                // Remove the product from the shopping cart if the count is 1
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }



        // Removes a product from the shopping cart
        public IActionResult Remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        //calculate the total price of the order
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
