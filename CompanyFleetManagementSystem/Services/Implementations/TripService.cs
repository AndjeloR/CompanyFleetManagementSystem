using CompanyFleetManagementSystem.Data;
using CompanyFleetManagementSystem.Models;
using CompanyFleetManagementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;


namespace CompanyFleetManagementSystem.Services.Implementations
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;
        public TripService(ApplicationDbContext context) => _context = context;

        public async Task<List<Trip>> GetAllTrips(string search = null, TripStatus? status = null)
        {
            var query = _context.Trips.Include(t => t.Driver).AsQueryable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(t => t.Name.Contains(search) || t.Description.Contains(search));
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);
            return await query.ToListAsync();
        }

        public async Task<List<Trip>> GetTripsByDriver(string driverId, TripStatus? status = null)
        {
            //FIX THIS BULLSHIT
            var query = _context.Trips.Where(t => t.DriverEmail == ).AsQueryable();
            if (status.HasValue) query = query.Where(t => t.Status == status.Value);
            return await query.ToListAsync();
        }

        public async Task CreateTrip(Trip trip) { _context.Trips.Add(trip); await _context.SaveChangesAsync(); }
        public async Task<Trip> GetTripById(int id) => await _context.Trips.Include(t => t.Driver).FirstOrDefaultAsync(m => m.Id == id);
        public async Task UpdateTrip(Trip trip) { _context.Update(trip); await _context.SaveChangesAsync(); }
        public async Task DeleteTrip(int id) { var t = await _context.Trips.FindAsync(id); _context.Trips.Remove(t); await _context.SaveChangesAsync(); }

        public async Task StartTrip(int id)
        {
            var t = await _context.Trips.FindAsync(id);
            if (t != null) { t.Status = TripStatus.InProgress; await _context.SaveChangesAsync(); }
        }

        public async Task FinishTrip(int id)
        {
            var t = await _context.Trips.FindAsync(id);
            if (t != null) { t.Status = TripStatus.Finished; t.ActualReturnTime = DateTime.Now; await _context.SaveChangesAsync(); }
        }
    }
}
