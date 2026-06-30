#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VOSG_NKBD.Areas.Identity.Pages.Account
{
    
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
     
        public void OnGet()
        {
        }
    }
}
