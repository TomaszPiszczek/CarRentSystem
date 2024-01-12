using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class Car
    {
        [Key]
        public int car_id { get; set; }
        public string brand { get; set; }
        public string model { get; set; }
        public int production_date { get; set; }
        public decimal daily_fee { get; set; }
        public bool avaliable { get; set; }
    }
}
