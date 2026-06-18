using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? city)
        {
            var locations = _context.Locations.Include(l => l.Places).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                locations = locations.Where(l =>
                    l.LocationName.Contains(search) ||
                    l.City.Contains(search));

            if (!string.IsNullOrEmpty(city))
                locations = locations.Where(l => l.City == city);

            ViewBag.Cities = await _context.Locations.Select(l => l.City).Distinct().ToListAsync();
            ViewBag.Search = search;
            ViewBag.City = city;
            ViewBag.Featured = await _context.Locations.Include(l => l.Places).Take(6).ToListAsync();

            return View(await locations.ToListAsync());
        }

        public async Task<IActionResult> City(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var locations = await _context.Locations
                .Include(l => l.Places)
                .Where(l => l.City == id)
                .ToListAsync();

            if (!locations.Any()) return RedirectToAction("Index");

            ViewBag.CityName = id;
            return View(locations);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}