using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Data;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Controllers
{
    [Authorize]
    public class ConfirmationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<VOSG_NKBDUser> _userManager;

        public ConfirmationsController(AppDbContext context, UserManager<VOSG_NKBDUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Confirmations
                .Include(c => c.Place).ThenInclude(p => p!.Location)
                .Include(c => c.User)
                .Include(c => c.Payment)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(c => c.MemberId == user!.Id);

            return View(await query.OrderByDescending(c => c.ConfirmationDate).ToListAsync());
        }

        public async Task<IActionResult> Book(int? id)
        {
            if (id == null) return NotFound();
            var place = await _context.Places
                .Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.PlaceID == id);
            if (place == null) return NotFound();

            var model = new Confirmation
            {
                PlaceID = place.PlaceID,
                StartTime = DateTime.Today.AddHours(9),
                EndTime = DateTime.Today.AddHours(17),
                TotalPrice = place.Price,
                Place = place
            };
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(
            [Bind("PlaceID,StartTime,EndTime,TotalPrice")] Confirmation confirmation)
        {
            var place = await _context.Places.Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.PlaceID == confirmation.PlaceID);
            if (place == null) return NotFound();

            confirmation.Place = place;
            ModelState.Remove("MemberId");
            ModelState.Remove("Place");
            ModelState.Remove("User");
            ModelState.Remove("Payment");

            if (ModelState.IsValid)
                return View("Confirm", confirmation);

            return View(confirmation);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            [Bind("PlaceID,StartTime,EndTime,TotalPrice")] Confirmation confirmation)
        {
            var user = await _userManager.GetUserAsync(User);
            var place = await _context.Places.Include(p => p.Location)
                .FirstOrDefaultAsync(p => p.PlaceID == confirmation.PlaceID);
            if (place == null) return NotFound();

            confirmation.MemberId = user!.Id;
            confirmation.ConfirmationDate = DateTime.Today;
            confirmation.TotalPrice = place.Price;

            _context.Confirmations.Add(confirmation);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                ConfirmationID = confirmation.ConfirmationID,
                VOSG_NKBDId = user.Id,
                PaymentAmount = confirmation.TotalPrice,
                PaymentDate = DateTime.Today,
                PaymentStatus = "Paid"
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking confirmed!";
            return RedirectToAction("Receipt", new { id = confirmation.ConfirmationID });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var conf = await _context.Confirmations
                .Include(c => c.Place).ThenInclude(p => p!.Location)
                .Include(c => c.Payment)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.ConfirmationID == id);
            if (conf == null) return NotFound();
            return View(conf);
        }
    }
} 
