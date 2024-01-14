using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class Rentals
    {
        [Key]
        public int rent_id { get; set; }

        public int client_id { get; set; }

        public int car_id { get; set; }

        [Required]
        public DateTime rent_date { get; set; }

        [Required]
        public DateTime return_date { get; set; }

        [Required]
        public decimal price { get; set; }

        [ForeignKey("client_id")]
        public virtual Client Client { get; set; }

        [ForeignKey("car_id")]
        public virtual Car Car { get; set; }
    }
}
