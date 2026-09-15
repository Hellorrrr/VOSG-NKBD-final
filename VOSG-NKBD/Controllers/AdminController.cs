using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // stats page for admins - written with actual SQL (JOIN + GROUP BY)
        // instead of LINQ, so there's real query-writing evidence for AS91902
        public async Task<IActionResult> Dashboard()
        {
            var revenueByLocation = await _context.Database.SqlQuery<LocationRevenueDto>($@"
                SELECT l.LocationName AS LocationName,
                       COUNT(c.ConfirmationID) AS BookingCount,
                       ISNULL(SUM(c.TotalPrice), 0) AS TotalRevenue
                FROM Locations l
                LEFT JOIN Places p ON p.LocationsID = l.LocationsID
                LEFT JOIN Confirmations c ON c.PlaceID = p.PlaceID
                GROUP BY l.LocationName
                ORDER BY TotalRevenue DESC
            ").ToListAsync();

            var popularPlaces = await _context.Database.SqlQuery<PopularPlaceDto>($@"
                SELECT TOP 5
                       p.PlaceName AS PlaceName,
                       l.LocationName AS LocationName,
                       COUNT(c.ConfirmationID) AS BookingCount
                FROM Places p
                JOIN Locations l ON l.LocationsID = p.LocationsID
                LEFT JOIN Confirmations c ON c.PlaceID = p.PlaceID
                GROUP BY p.PlaceName, l.LocationName
                ORDER BY BookingCount DESC
            ").ToListAsync();

            ViewBag.RevenueByLocation = revenueByLocation;
            ViewBag.PopularPlaces = popularPlaces;
            ViewBag.TotalBookings = await _context.Confirmations.CountAsync();
            ViewBag.TotalRevenue = await _context.Confirmations.SumAsync(c => (decimal?)c.TotalPrice) ?? 0;

            return View();
        }
    }
}