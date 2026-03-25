using CompanyFleetManagementSystem.Models;
using CompanyFleetManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyFleetManagementSystem.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ITripService tripService;

        public TripsController(ITripService tripService)
        {
            this.tripService = tripService;
        }

        public async Task<IActionResult> Index(string search, TripStatus? status)
        {
            var trips = await tripService.GetAllTrips(search, status);
            return View(trips);
        }

        [Authorize(Roles = "Administrator, Dispatcher")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, Dispatcher")]
        public async Task<IActionResult> Create(Trip trip)
        {
            if (!ModelState.IsValid)
                return View(trip);

            trip.DispatcherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await tripService.CreateTrip(trip);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> MyTrips()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var trips = await tripService.GetTripsByDriver(userId);
            return View(trips);
        }

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Start(int id)
        {
            await tripService.StartTrip(id);
            return RedirectToAction("MyTrips");
        }

        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> Finish(int id)
        {
            await tripService.FinishTrip(id);
            return RedirectToAction("MyTrips");
        }
    }
}