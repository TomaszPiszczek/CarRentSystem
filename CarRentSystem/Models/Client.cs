using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class Client
    {
        [Key]
        public int client_id { get; set; }

        public int user_id { get; set; }

        [ForeignKey("user_id")]
        public virtual User User { get; set; }
    }
}
