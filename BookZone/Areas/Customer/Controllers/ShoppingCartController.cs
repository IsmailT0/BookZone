using BookZone.Attributes;
using BookZone.DataAccess.Repository.IRepository;
using BookZone.Models;
using BookZone.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookZone.Areas.Customer.Controllers
{
    [Area("Customer")]
    [CustomAuthorize]
    public class ShoppingCartController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        public ShoppingCartVM shoppinCartVM { get; set; }
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM shoppingCartVM;
            if (int.TryParse(userId, out int parsedUserId))
            {
                shoppingCartVM = new ShoppingCartVM
                {
                    ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == parsedUserId, includeProperties: "Product"),
                    OrderTotal = 0
                };
            }
            else
            {
                // Handle the case where userId is not a valid int
                return BadRequest("Invalid user ID");
            }
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
               cart.Price = calculateOrderTotal(cart);
                shoppingCartVM.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
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
