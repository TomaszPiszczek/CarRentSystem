using CarRentSystem.Models;
using CarRentSystem.Nowy_folder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class RentController : Controller
{
    private readonly RentCarDb _context;

    public RentController(RentCarDb context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        
        var cars = _context.Cars.ToList();
        Console.WriteLine(cars.Count);
        var rentals = _context.Rentals.ToList();

        
        ViewBag.Cars = cars;
        ViewBag.Rentals = rentals;

        return View("Index");
    }
  public async Task<IActionResult> RentCar(int carId, DateTime startDate, DateTime endDate)
    {
        // Sprawdź, czy data wynajmu jest w przyszłości
        if (startDate < DateTime.UtcNow.Date || endDate < DateTime.UtcNow.Date || endDate < startDate)
        {
            ViewBag.ErrorMessage = "Błędna data" ;
            return View("Index");
        }

        
        var isCarAvailable = IsCarAvailable(carId, startDate, endDate);
        if (!isCarAvailable)
        {
            ViewBag.ErrorMessage = "Auto jest niedostępne w wybranym okresie.";
            return View("Index");
        }

        
        var clientIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
        if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int userId))
        {
            ViewBag.ErrorMessage = "Musisz byc zalogowany";
            return View("Index");
        }

        Console.WriteLine(userId);

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.user_id == userId);

                if (client == null)
                {
                    
                    ViewBag.ErrorMessage = "Nie znaleziono klienta dla danego użytkownika.";
                    return View("Index");
                }

                
                var rental = new Rentals
                {
                    client_id = client.client_id,
                    car_id = carId,
                    rent_date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                    return_date = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                    price = CalculatePrice(carId, startDate, endDate)
                };

                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();

                
                transaction.Commit();

                
                var car = _context.Cars.FirstOrDefault(c => c.car_id == carId);
                var rentedMessage = $"Wynajęto samochód, cena: {rental.price}, marka: {car?.brand}, model: {car?.model}";

                
                ViewBag.RentedMessage = rentedMessage;

                return View("Index");
            }
            catch (Exception ex)
            {
                
                transaction.Rollback();

                
                Console.WriteLine($"Błąd podczas przetwarzania transakcji: {ex.Message}");
                ViewBag.ErrorMessage = "Wystąpił błąd podczas przetwarzania transakcji.";
                return View("Index");
            }
        }
    }



    private bool IsCarAvailable(int carId, DateTime startDate, DateTime endDate)
{
    
    var existingRentals = _context.Rentals
        .Where(r => r.car_id == carId)
        .ToList();

    
    foreach (var rental in existingRentals)
    {
        if ((startDate >= rental.rent_date && startDate < rental.return_date) ||
            (endDate > rental.rent_date && endDate <= rental.return_date) ||
            (startDate <= rental.rent_date && endDate >= rental.return_date))
        {
            return false;
        }
    }

    return true;
}


    private decimal CalculatePrice(int carId, DateTime startDate, DateTime endDate)
    {
        var car = _context.Cars.FirstOrDefault(c => c.car_id == carId);

        if (car == null)
        {
            
            ViewBag.ErrorMessage = "Nie znaleziono samochodu dla danego ID.";
            return 0; 
        }

        var days = (endDate - startDate).Days;
        var dailyRate = car.daily_fee; 
        return days * dailyRate;
    }

    

    public IActionResult GetOccupiedDates(int carId)
    {
        var occupiedDates = GetOccupiedDatesForCar(carId);

        
        ViewBag.OccupiedDates = occupiedDates;

        return View("OccupiedDates");
    }

    public List<DateTime> GetOccupiedDatesForCar(int carId)
    {
        var occupiedDates = _context.Rentals
            .Where(r => r.car_id == carId)
            .OrderBy(r => r.rent_date) 
            .ThenBy(r => r.return_date) 
            .ToList()
            .SelectMany(r => Enumerable.Range(0, (int)(r.return_date - r.rent_date).TotalDays + 1)
                .Select(offset => r.rent_date.AddDays(offset)))
            .Distinct() 
            .OrderBy(date => date) 
            .ToList();

        return occupiedDates;
    }
    [HttpPost]
    public IActionResult GetRentals()
    {
        
        return RedirectToAction("MyRentals", "MyRentals");
    }

    [HttpPost]
    public IActionResult DeleteCar(int carId)
    {

        if (!IsUserAdmin())
        {
            ViewBag.ErrorMessage = "Nie masz uprawnień do usuwania samochodów.";
            return View("Index");
        }

        var car = _context.Cars.Include(c => c.Rentals).FirstOrDefault(c => c.car_id == carId);

        if (car == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono samochodu o podanym ID.";
            return View("Index");
        }

        
        if (!car.avaliable)
        {
            ViewBag.ErrorMessage = "Nie można usunąć niedostępnego samochodu.";
            return View("Index");
        }

        
        if (car.Rentals.Any(r => r.return_date == null || r.return_date > DateTime.UtcNow))
        {
            ViewBag.ErrorMessage = "Nie można usunąć samochodu, który jest aktualnie wynajmowany.";
            return View("Index");
        }

        _context.Cars.Remove(car);
        _context.SaveChanges();

        ViewBag.SuccessMessage = "Samochód został pomyślnie usunięty.";

        
        var cars = _context.Cars.ToList();
        ViewBag.Cars = cars;

        return View("Index");
    }

    [HttpPost]
    public IActionResult ToggleAvailability(int carId)
    {
        if (!IsUserAdmin())
        {
            ViewBag.ErrorMessage = "Nie masz uprawnień do zmiany dostępności samochodów.";
            return View("Index");
        }
        var car = _context.Cars.FirstOrDefault(c => c.car_id == carId);

        if (car == null)
        {
            ViewBag.ErrorMessage = "Nie znaleziono samochodu o podanym ID.";
            return View("Index");
        }

        
        car.avaliable = !car.avaliable;
        _context.SaveChanges();

        ViewBag.SuccessMessage = $"Dostępność samochodu {car.brand} {car.model} została pomyślnie zmieniona.";

        
        var cars = _context.Cars.ToList();
        ViewBag.Cars = cars;

        return View("Index");
    }
    [HttpPost]
    public IActionResult AddCar(string brand, string model, int productionDate, decimal dailyFee, bool available)
    {
        if (!IsUserAdmin())
        {
            ViewBag.ErrorMessage = "Nie masz uprawnień do dodania samochodów.";
            return View("Index");
        }
        try
        {
            var newCar = new Car
            {
                brand = brand,
                model = model,
                production_date = productionDate,
                daily_fee = dailyFee,
                avaliable = available
            };

            _context.Cars.Add(newCar);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Samochód został pomyślnie dodany.";
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = $"Wystąpił błąd podczas dodawania samochodu: {ex.Message}";
        }

        
        var cars = _context.Cars.ToList();
        ViewBag.Cars = cars;

        return View("Index");
    }

    private bool IsUserAdmin()
    {
        var isAdminClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "IsAdmin");
        return isAdminClaim != null && bool.TryParse(isAdminClaim.Value, out bool isAdmin) && isAdmin;
    }
    [HttpPost]
    public IActionResult Logout()
    {

        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        
        return RedirectToAction("Index", "Home");
    }

}
