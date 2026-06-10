using Microsoft.AspNetCore.Identity;

namespace VOSG_NKBD.Models
{
    public class VOSG_NKBDUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}