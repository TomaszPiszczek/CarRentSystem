using CarRentSystem.Models;
using CarRentSystem.Nowy_folder;
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
        // Pobierz listę samochodów i ich dostępność
        var cars = _context.Cars.ToList();
        Console.WriteLine(cars.Count);
        var rentals = _context.Rentals.ToList();

        // Przekaż dane do widoku
        ViewBag.Cars = cars;
        ViewBag.Rentals = rentals;

        return View("Index");
    }
    public async Task<IActionResult> RentCar(int carId, DateTime startDate, DateTime endDate)
    {
        // Sprawdź dostępność auta w wybranym okresie
        var isCarAvailable = IsCarAvailable(carId, startDate, endDate);
        if (!isCarAvailable)
        {
            ViewBag.ErrorMessage = "Auto jest niedostępne w wybranym okresie.";
            return View("Index");
        }

        // Pobierz identyfikator klienta z claimsów
        var clientIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "id");
        if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int userId))
        {
            ViewBag.ErrorMessage = "Nieprawidłowe dane użytkownika.";
            return View("Index");
        }

        Console.WriteLine(userId);

        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                // Wyciągnij client_id dla user_id z bazy danych
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.user_id == userId);

                if (client == null)
                {
                    // Obsłuż sytuację, gdy nie istnieje klient o podanym user_id
                    ViewBag.ErrorMessage = "Nie znaleziono klienta dla danego użytkownika.";
                    return View("Index");
                }

                // Dodaj nowy wynajem
                var rental = new Rentals
                {
                    client_id = client.client_id,
                    car_id = carId,
                    rent_date = DateTime.SpecifyKind(startDate, DateTimeKind.Utc),
                    return_date = DateTime.SpecifyKind(endDate, DateTimeKind.Utc),
                    price = CalculatePrice(startDate, endDate)
                };

                _context.Rentals.Add(rental);
                await _context.SaveChangesAsync();

                // Jeśli wszystko przebiegło pomyślnie, zatwierdź transakcję
                transaction.Commit();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // W przypadku błędu, anuluj transakcję
                transaction.Rollback();

                // Dodaj więcej informacji o błędzie do logów
                Console.WriteLine($"Błąd podczas przetwarzania transakcji: {ex.Message}");
                ViewBag.ErrorMessage = "Wystąpił błąd podczas przetwarzania transakcji.";
                return View("Index");
            }
        }
    }



   private bool IsCarAvailable(int carId, DateTime startDate, DateTime endDate)
{
    // Sprawdź dostępność auta w wybranym okresie
    var existingRentals = _context.Rentals
        .Where(r => r.car_id == carId)
        .ToList();

    // Sprawdź, czy nowa rezerwacja nakłada się na istniejące rezerwacje
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
   

    private decimal CalculatePrice(DateTime startDate, DateTime endDate)
    {
        // Przykładowa logika kalkulacji ceny wynajmu
        // Możesz dostosować ją zgodnie z własnymi wymaganiami
        var days = (endDate - startDate).Days;
        var dailyRate = 100; // Przykładowa stawka dzienna
        return days * dailyRate;
    }
}
