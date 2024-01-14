using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentSystem.Models;
using CarRentSystem.Nowy_folder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

[Authorize]
public class MyRentalsController : Controller
{
    private readonly RentCarDb _context;

    public MyRentalsController(RentCarDb context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult MyRentals()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            ViewBag.ErrorMessage = "Nieprawidłowe dane użytkownika.";
            return View("aa");
        }

        var client = _context.Clients.FirstOrDefault(c => c.user_id == userId);

        if (client == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono klienta dla danego użytkownika." + userId;
            return View("MyRentals");
        }

        var clientId = client.client_id;

        var myRentals = _context.Rentals
            .Include(r => r.Car)
            .Where(r => r.client_id == clientId)
            .ToList();

        var myRentalViewModels = myRentals.Select(r => new MyRentalViewModel
        {
            RentalId = r.rent_id,
            Brand = r.Car?.brand,
            Model = r.Car?.model,
            StartDate = r.rent_date,
            EndDate = r.return_date,
            Price = r.price,
            CanCancel = r.rent_date > DateTime.UtcNow.AddDays(2)
        })
        .ToList();

        // Przekaż dane do widoku przez ViewBag
        ViewBag.MyRentals = myRentalViewModels;

        return View("MyRentals");
    }
    [HttpPost]
    public IActionResult CancelRental(int rentalId)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            ViewBag.ErrorMessage = "Musisz byc zalogowany";
            return View("MyRentals");
        }

        var client = _context.Clients.FirstOrDefault(c => c.user_id == userId);

        if (client == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono klienta dla danego użytkownika." + userId;
            return View("MyRentals");
        }

        var rental = _context.Rentals
            .FirstOrDefault(r => r.rent_id == rentalId && r.client_id == client.client_id);

        if (rental == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono wynajmu o podanym identyfikatorze dla danego użytkownika.";
            return View("MyRentals");
        }

        _context.Rentals.Remove(rental);
        _context.SaveChanges();

        ViewBag.SuccessMessage = "Wynajem został pomyślnie anulowany.";

        // Ponownie pobierz dane wynajmów po anulowaniu
        var myRentals = _context.Rentals
            .Include(r => r.Car)
            .Where(r => r.client_id == client.client_id)
            .ToList();

        var myRentalViewModels = myRentals.Select(r => new MyRentalViewModel
        {
            RentalId = r.rent_id,
            Brand = r.Car?.brand,
            Model = r.Car?.model,
            StartDate = r.rent_date,
            EndDate = r.return_date,
            Price = r.price,
            CanCancel = r.rent_date > DateTime.UtcNow.AddDays(2)
        })
        .ToList();

        // Przekaż dane do widoku przez ViewBag
        ViewBag.MyRentals = myRentalViewModels;

        // W kontrolerze HomeController
       

        return View("MyRentals");
    }
   


}