using CompanyFleetManagementSystem.Models;

namespace CompanyFleetManagementSystem.Services.Interfaces
{
    public interface ITripService
    {
        Task<List<Trip>> GetAllTrips(string search = null, TripStatus? status = null);
        Task<List<Trip>> GetTripsByDriver(string driverId, TripStatus? status = null);
        Task<Trip> GetTripById(int id);
        Task CreateTrip(Trip trip);
        Task UpdateTrip(Trip trip);
        Task DeleteTrip(int id);
        Task StartTrip(int id);
        Task FinishTrip(int id);
    }
}
