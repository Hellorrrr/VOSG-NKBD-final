namespace VOSG_NKBD.Models
{
    // these are query-result shapes, not database tables - used only to receive
    // the columns coming back from the raw SQL in AdminController.Dashboard()
    public record LocationRevenueDto(string LocationName, int BookingCount, decimal TotalRevenue);
    public record PopularPlaceDto(string PlaceName, string LocationName, int BookingCount);
}