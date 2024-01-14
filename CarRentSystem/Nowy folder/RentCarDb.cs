using CarRentSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentSystem.Nowy_folder
{
    public class RentCarDb:DbContext
    {
        public RentCarDb(DbContextOptions<RentCarDb> options) : base(options)
        {
        }

        public DbSet<Car> Cars { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rentals> Rentals { get; set; }
        public DbSet<Client> Clients { get; set; }  
    }
}
