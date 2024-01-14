using CarRentSystem.Models;
using CarRentSystem.Nowy_folder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CarRentSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly RentCarDb _context;

        public HomeController(RentCarDb context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login(User user)
        {
            // Check if user with provided credentials exists in the database
            var existingUser = _context.Users.FirstOrDefault(u => u.name == user.name && u.password == HashPassword(user.password));

            if (existingUser != null)
            {
                // If successful, set authentication cookie or perform other login actions
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, existingUser.name),
            new Claim("password", existingUser.password), // Dodaj claim z hasłem
           new Claim("id", Convert.ToString(existingUser.user_id)),
           new Claim("IsAdmin", existingUser.is_admin.ToString()),
            // Dodaj inne claimy w razie potrzeby
        };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20), // Czas ważności sesji
                    IsPersistent = true, // Czy sesja powinna być trwała nawet po zamknięciu przeglądarki
                    RedirectUri = "/Home/rent.cshtml" // Przekierowanie po poprawnym zalogowaniu
                };

                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Rent");
            }

            // If login fails, you may want to display an error message
            ViewBag.LoginErrorMessage = "Invalid username or password";
            return View("Index");
        }



        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(u => u.name == user.name))
            {
                ViewBag.RegisterErrorMessage = "Username already exists";
                return View("index");
            }

            // Hash the password before saving
            user.password = HashPassword(user.password);

            // Utwórz nowego klienta i przypisz mu user_id
            var client = new Client { User = user };
            user.Client = client;

            // Zapisz użytkownika wraz z klientem do bazy danych
            _context.Users.Add(user);
            _context.SaveChanges();

            // For simplicity, redirect to the Index action
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Rent()
        {
            // Logika dla strony rent.cshtml
            return View("Rent/Index");
        }

        [HttpPost]
        public IActionResult ContinueWithoutLogin()
        {
        
            return RedirectToAction("Rent/Index");
        }

        // Helper method to hash passwords using SHA-256
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
