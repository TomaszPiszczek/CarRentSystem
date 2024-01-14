
using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class User
    {
        [Key]
        public int user_id { get; set; }

        [Required]
        [MaxLength(50)]
        public string name { get; set; }

        [Required]
        [MaxLength(50)]
        public string surname { get; set; }
        [Required]
        [MaxLength(50)]
        public string password { get; set; }

        [Required]
        [MaxLength(100)]
        public string adress { get; set; }

        [Required]
        [MaxLength(15)]
        public string phone_number { get; set; }

        [Required]
        public bool is_admin { get; set; }

        public virtual Client Client { get; set; }
    }
}
