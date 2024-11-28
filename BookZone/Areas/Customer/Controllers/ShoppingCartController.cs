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


            return View(shoppingCartVM);
        }
    }
}
