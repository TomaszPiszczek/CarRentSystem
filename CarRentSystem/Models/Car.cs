using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class Car
    {
        [Key]
        public int car_id { get; set; }

        [Required]
        [MaxLength(50)]
        public string brand { get; set; }

        [Required]
        [MaxLength(50)]
        public string model { get; set; }

        [Required]
        public int production_date { get; set; }

        [Required]
        public decimal daily_fee { get; set; }

        [Required]
        public bool avaliable { get; set; }
    }
}
