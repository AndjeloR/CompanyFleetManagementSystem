using CompanyFleetManagementSystem.Data;
using CompanyFleetManagementSystem.Models;
using CompanyFleetManagementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CompanyFleetManagementSystem.Controllers
{
    [Authorize]
    public class TripsController : Controller
    {
        private readonly ITripService tripService;
        private readonly UserManager<ApplicationUser> userManager;

        public TripsController(ITripService tripService, UserManager<ApplicationUser> userManager)
        {
            this.tripService = tripService;
            this.userManager = userManager;
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

            // Look up the driver by email and set DriverId
            if (!string.IsNullOrEmpty(trip.DriverEmail))
            {
                var driver = await userManager.FindByEmailAsync(trip.DriverEmail);
                if (driver == null)
                {
                    ModelState.AddModelError("DriverEmail", "Driver with this email not found.");
                    return View(trip);
                }
                trip.DriverId = driver.Id;
            }
            else
            {
                ModelState.AddModelError("DriverEmail", "Driver email is required.");
                return View(trip);
            }

            trip.DispatcherId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validate that the dispatcher exists in the database
            var dispatcher = await userManager.FindByIdAsync(trip.DispatcherId);
            if (dispatcher == null)
            {
                ModelState.AddModelError("", "Current user not found in the database.");
                return View(trip);
            }

            trip.Status = TripStatus.Waiting;

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