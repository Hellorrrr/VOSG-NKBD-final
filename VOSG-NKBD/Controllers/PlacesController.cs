using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Controllers
{
    // CRUD for the individual bookable spaces inside a location.
    // basically the same pattern as LocationsController - everyone can browse,
    // only Admin gets the add/edit/delete buttons
    public class PlacesController : Controller
    {
        private readonly AppDbContext _context;

        public PlacesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchString, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;

            var places = _context.Places.Include(p => p.Location).AsQueryable();

            // let people search by either the place name or the location it belongs to,
            // since from the admin side you usually remember the venue first
            if (!string.IsNullOrEmpty(searchString))
                places = places.Where(p =>
                    p.PlaceName.Contains(searchString) ||
                    p.Location!.LocationName.Contains(searchString));

            places = places.OrderBy(p => p.Location!.LocationName).ThenBy(p => p.PlaceName);

            return View(await PaginatedList<Place>.CreateAsync(places.AsNoTracking(), pageNumber ?? 1, 10));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? locationsId)
        {
            // if we got here from the "Add Space" button on a location's details page,
            // pre-select that location instead of making the admin pick it again
            ViewBag.Locations = new SelectList(
                await _context.Locations.OrderBy(l => l.LocationName).ToListAsync(),
                "LocationsID", "LocationName", locationsId);

            var model = new Place();
            if (locationsId.HasValue) model.LocationsID = locationsId.Value;
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("PlaceName,Description,ImageUrl,Price,LocationsID")] Place place)
        {
            if (ModelState.IsValid)
            {
                _context.Add(place);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Space '{place.PlaceName}' created.";
                return RedirectToAction(nameof(Index));
            }

            // validation failed - gotta rebuild the dropdown or the view blows up,
            // the ViewBag doesnt carry over automatically on a failed post
            ViewBag.Locations = new SelectList(
                await _context.Locations.OrderBy(l => l.LocationName).ToListAsync(),
                "LocationsID", "LocationName", place.LocationsID);
            return View(place);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var place = await _context.Places.FindAsync(id);
            if (place == null) return NotFound();

            ViewBag.Locations = new SelectList(
                await _context.Locations.OrderBy(l => l.LocationName).ToListAsync(),
                "LocationsID", "LocationName", place.LocationsID);
            return View(place);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("PlaceID,PlaceName,Description,ImageUrl,Price,LocationsID")] Place place)
        {
            if (id != place.PlaceID) return NotFound();

            if (ModelState.IsValid)
            {
                try { _context.Update(place); await _context.SaveChangesAsync(); }
                catch (DbUpdateConcurrencyException)
                {
                    // same deal as LocationsController.Edit - only throw if the row
                    // is genuinely gone, not just because of a stale concurrency token
                    if (!_context.Places.Any(p => p.PlaceID == id)) return NotFound();
                    throw;
                }
                TempData["Success"] = $"Space '{place.PlaceName}' updated.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Locations = new SelectList(
                await _context.Locations.OrderBy(l => l.LocationName).ToListAsync(),
                "LocationsID", "LocationName", place.LocationsID);
            return View(place);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var place = await _context.Places.Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.PlaceID == id);
            if (place == null) return NotFound();
            return View(place);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var place = await _context.Places.FindAsync(id);
            if (place != null)
            {
                // block the delete if someone already booked this place - otherwise
                // their Confirmation row points at a Place that no longer exists
                var hasBookings = await _context.Confirmations.AnyAsync(c => c.PlaceID == id);
                if (hasBookings)
                {
                    TempData["Error"] = "This space has existing bookings and cannot be deleted.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Places.Remove(place);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Space deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}