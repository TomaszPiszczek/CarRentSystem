namespace CarRentSystem.Models
{
    public class MyRentalViewModel
    {
        public int RentalId { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public bool CanCancel { get; set; }
    }

}
