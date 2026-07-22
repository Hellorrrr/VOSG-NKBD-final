using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Controllers
{
    // CRUD for locations - anyone can view, only admins can create/edit/delete
    public class LocationsController : Controller
    {
        private readonly AppDbContext _context;

        public LocationsController(AppDbContext context)
        {
            _context = context;
        }

        // search + sort + paging all happen in this one action
        public async Task<IActionResult> Index(string? searchString, int? pageNumber, string? currentFilter, string? sortOrder)
        {
            // these get used by the column header links in the view -
            // clicking Name toggles between a-z and z-a
            ViewData["NameSort"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CitySort"] = sortOrder == "city" ? "city_desc" : "city";
            ViewData["CurrentFilter"] = searchString ?? currentFilter;
            ViewData["CurrentSort"] = sortOrder;

            // new search = jump back to page 1, otherwise keep the old
            // filter when moving between pages (currentFilter carries it over)
            if (searchString != null) pageNumber = 1;
            else searchString = currentFilter;

            var locations = _context.Locations.Include(l => l.Places).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
                locations = locations.Where(l =>
                    l.LocationName.Contains(searchString) ||
                    l.Address.Contains(searchString) ||
                    l.City.Contains(searchString));

            locations = sortOrder switch
            {
                "name_desc" => locations.OrderByDescending(l => l.LocationName),
                "city" => locations.OrderBy(l => l.City),
                "city_desc" => locations.OrderByDescending(l => l.City),
                _ => locations.OrderBy(l => l.LocationName),
            };

            // AsNoTracking = faster because we're only reading, not editing
            return View(await PaginatedList<Location>.CreateAsync(locations.AsNoTracking(), pageNumber ?? 1, 10));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var location = await _context.Locations
                .Include(l => l.Places)
                .FirstOrDefaultAsync(l => l.LocationsID == id);
            if (location == null) return NotFound();
            return View(location);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        // Bind() whitelists the fields so nobody can post extra ones (overposting)
        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("LocationName,Address,Suburb,City,Country,PostalCode,PhoneNumber")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Location '{location.LocationName}' created.";
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var location = await _context.Locations.FindAsync(id);
            if (location == null) return NotFound();
            return View(location);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("LocationsID,LocationName,Address,Suburb,City,Country,PostalCode,PhoneNumber")] Location location)
        {
            if (id != location.LocationsID) return NotFound();
            if (ModelState.IsValid)
            {
                try { _context.Update(location); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException)
                {
                    // happens if someone deleted the location while we were editing it
                    if (!_context.Locations.Any(l => l.LocationsID == id)) return NotFound();
                    throw;
                }
                TempData["Success"] = $"Location '{location.LocationName}' updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(location);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var location = await _context.Locations.FirstOrDefaultAsync(l => l.LocationsID == id);
            if (location == null) return NotFound();
            return View(location);
        }

        // ActionName("Delete") because this method cant also be called Delete
        // (same name + same parameters as the GET one isnt allowed)
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null) _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Location deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}