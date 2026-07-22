using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VOSG_NKBD.Models;

namespace VOSG_NKBD.Data
{
    // seeds the database with starting data the first time the app runs:
    // roles, an admin account, test members, locations, places and some bookings.
    // runs on every startup but skips itself if data already exists
    public static class DatabaseSeed
    {
        public static async Task SeedAsync(IApplicationBuilder app)
        {
            // create a scope to grab the services we need (DI)
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<VOSG_NKBDUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // SECURITY: passwords come from user secrets (dev) or environment
            // variables (production) - NOT from source code. no fallback on
            // purpose: if the secret is missing the app throws instead of
            // silently creating accounts with a password visible in the repo
            var adminPassword = config["SeedData:AdminPassword"]
                ?? throw new InvalidOperationException("SeedData:AdminPassword is not configured. Set it in user secrets.");
            var memberPassword = config["SeedData:MemberPassword"]
                ?? throw new InvalidOperationException("SeedData:MemberPassword is not configured. Set it in user secrets.");

            // apply any pending EF migrations - creates the database if it
            // doesn't exist yet (better than EnsureCreated because it keeps
            // the migration history working)
            await context.Database.MigrateAsync();

            // make sure both roles exist before we assign anyone to them
            foreach (var role in new[] { "Admin", "Member" })
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // if locations already exist we've seeded before - stop here
            if (context.Locations.Any()) return;

            // ---- sample venues ----
            var locations = new Location[]
            {
                new() { LocationName = "Auckland Showgrounds",                        Address = "217 Green Lane West",          Suburb = "Epsom",          City = "Auckland",           Country = "New Zealand",  PostalCode = "1051",     PhoneNumber = "+6496230092"  },
                new() { LocationName = "Nathan Phillips Square",                      Address = "100 Queen Street West",        Suburb = "Central City",   City = "Toronto",            Country = "Canada",       PostalCode = "M5H2N2",   PhoneNumber = "+14163922489" },
                new() { LocationName = "Dr Phillips Center",                          Address = "445 South Magnolia Avenue",    Suburb = "Central City",   City = "Orlando",            Country = "USA",          PostalCode = "32801",    PhoneNumber = "+14073586603" },
                new() { LocationName = "Federation Square",                           Address = "Swanston and Flinders Street", Suburb = "Central City",   City = "Melbourne",          Country = "Australia",    PostalCode = "3000",     PhoneNumber = "+61396551900" },
                new() { LocationName = "Finlandia Hall",                              Address = "Mannerheimintie 13",           Suburb = "Central City",   City = "Helsinki",           Country = "Finland",      PostalCode = "00100",    PhoneNumber = "+35894024100" },
                new() { LocationName = "Binh Duong Convention and Exhibition Center", Address = "10 Huu Nghi Boulevard",        Suburb = "Phu Chanh Ward", City = "Ho Chi Minh City",   Country = "Vietnam",      PostalCode = "75000",    PhoneNumber = "+842742222888" },
            };
            context.Locations.AddRange(locations);
            await context.SaveChangesAsync(); // save now so LocationsID values exist for the places below

            // ---- one bookable space per venue ----
            var places = new Place[]
            {
                new() { PlaceName = "Main Hall A",          Description = "Large event hall",                                                                Price =  500m,  LocationsID = locations[0].LocationsID, ImageUrl = "/images/hall-auckland.jpg"  },
                new() { PlaceName = "Conference B",         Description = "Conference room",                                                                 Price =  200m,  LocationsID = locations[1].LocationsID, ImageUrl = "/images/hall-toronto.jpg"   },
                new() { PlaceName = "Outdoor Stage",        Description = "Open air stage",                                                                  Price =  350m,  LocationsID = locations[2].LocationsID, ImageUrl = "/images/hall-orlando.jpg"   },
                new() { PlaceName = "Gallery Room",         Description = "Art gallery space",                                                               Price =  150m,  LocationsID = locations[3].LocationsID, ImageUrl = "/images/hall-melbourne.jpg" },
                new() { PlaceName = "Concert Hall",         Description = "Concert venue",                                                                   Price =  800m,  LocationsID = locations[4].LocationsID, ImageUrl = "/images/hall-helsinki.jpg"  },
                new() { PlaceName = "Grand Convention Hall",Description = "World-class convention hall for large-scale events, exhibitions and conferences", Price = 1200m,  LocationsID = locations[5].LocationsID, ImageUrl = "/images/hall-binhduong.jpg" },
            };
            context.Places.AddRange(places);
            await context.SaveChangesAsync();

            // ---- admin account ----
            var adminEmail = "admin@vosg.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new VOSG_NKBDUser { UserName = adminEmail, Email = adminEmail, FirstName = "Admin", LastName = "VOSG", EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }

            // ---- test member accounts ----
            var members = new (string First, string Last, string Email)[]
            {
                ("Nicolas", "Jackson",  "nicolas@vosg.com"),
                ("Jenny",   "Monroe",   "jenny@vosg.com"),
                ("Alice",   "Sydney",   "alice@vosg.com"),
                ("Bobby",   "Gordon",   "bobby@vosg.com"),
                ("Emma",    "Hathaway", "emma@vosg.com"),
            };
            var createdMembers = new List<VOSG_NKBDUser>();
            foreach (var (first, last, email) in members)
            {
                var existing = await userManager.FindByEmailAsync(email);
                if (existing == null)
                {
                    var user = new VOSG_NKBDUser { UserName = email, Email = email, FirstName = first, LastName = last, EmailConfirmed = true };
                    var r = await userManager.CreateAsync(user, memberPassword);
                    if (r.Succeeded) { await userManager.AddToRoleAsync(user, "Member"); createdMembers.Add(user); }
                }
                else createdMembers.Add(existing);
            }

            // need at least 4 members for the sample bookings below
            if (createdMembers.Count < 4) return;

            // ---- sample bookings so the tables aren't empty on first run ----
            var confirmations = new Confirmation[]
            {
                new() { MemberId = createdMembers[0].Id, PlaceID = places[0].PlaceID, ConfirmationDate = DateTime.Today, StartTime = new DateTime(2026, 9, 14, 9, 0, 0), EndTime = new DateTime(2026, 9, 14, 17, 0, 0), TotalPrice = places[0].Price },
                new() { MemberId = createdMembers[1].Id, PlaceID = places[1].PlaceID, ConfirmationDate = DateTime.Today, StartTime = new DateTime(2026, 9, 15, 9, 0, 0), EndTime = new DateTime(2026, 9, 15, 17, 0, 0), TotalPrice = places[1].Price },
                new() { MemberId = createdMembers[2].Id, PlaceID = places[2].PlaceID, ConfirmationDate = DateTime.Today, StartTime = new DateTime(2026, 9, 16, 9, 0, 0), EndTime = new DateTime(2026, 9, 16, 17, 0, 0), TotalPrice = places[2].Price },
                new() { MemberId = createdMembers[3].Id, PlaceID = places[3].PlaceID, ConfirmationDate = DateTime.Today, StartTime = new DateTime(2026, 9, 19, 9, 0, 0), EndTime = new DateTime(2026, 9, 19, 17, 0, 0), TotalPrice = places[3].Price },
            };
            context.Confirmations.AddRange(confirmations);
            await context.SaveChangesAsync();

            // ---- matching "Paid" payment record for each sample booking ----
            var payments = confirmations.Select(c => new Payment
            {
                ConfirmationID = c.ConfirmationID,
                VOSG_NKBDId = c.MemberId,
                PaymentAmount = c.TotalPrice,
                PaymentDate = DateTime.Today,
                PaymentStatus = "Paid"
            }).ToArray();
            context.Payments.AddRange(payments);
            await context.SaveChangesAsync();
        }
    }
}