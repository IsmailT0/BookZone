using BookZone.DataAccess.Repository.IRepository;
using BookZone.Models;
using BookZone.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookZone.Controllers
{
    public class AuthController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();  
        }


        [HttpPost]
        public IActionResult Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the view with validation errors
            }

            // Check if user already exists
            var existingUser = _unitOfWork.Users.GetByEmail(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View(model);
            }

            // Create a new user
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            var user = new User
            {
                Email = model.Email,
                PasswordHash = hashedPassword,
                UserType = "Customer" // Or "Admin"
            };

            _unitOfWork.Users.Add(user);
            _unitOfWork.Save();

            return RedirectToAction("Login");
        }


        [HttpPost]
        public IActionResult Login(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _unitOfWork.Users.GetByEmail(model.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Set session or token
            HttpContext.Session.SetString("UserType", user.UserType);
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Redirect based on UserType
            if (user.UserType == "Admin")
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
