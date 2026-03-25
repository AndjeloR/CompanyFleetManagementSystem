using System.ComponentModel.DataAnnotations;

namespace CompanyFleetManagementSystem.Models
{
    public class Trip
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string Description { get; set; }
        [Required] public string Route { get; set; }
        [Required] public DateTime DepartureTime { get; set; }
        [Required] public DateTime ExpectedReturnTime { get; set; }
        public DateTime? ActualReturnTime { get; set; }
        public TripStatus Status { get; set; }
        public string? DriverEmail { get; set; }
        public string DriverId { get; set; }
        public virtual ApplicationUser? Driver { get; set; }
        public string? DispatcherId { get; set; }
        public virtual ApplicationUser? Dispatcher { get; set; }
    }
}
