using System.ComponentModel.DataAnnotations;

namespace VOSG_NKBD.Models
{
    public class Location
    {
        [Key]
        public int LocationsID { get; set; }

        [Required, MinLength(2), MaxLength(50)]
        [Display(Name = "Location Name")]
        public string LocationName { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(60)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(30)]
        public string Suburb { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(30)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Country { get; set; } = string.Empty;

        [Required, MinLength(2), MaxLength(20)]
        [RegularExpression(@"^[A-Z0-9 -]+$", ErrorMessage = "Invalid postal code.")]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\+?\d[\d\-]{6,14}$", ErrorMessage = "Invalid phone number format.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public ICollection<Place>? Places { get; set; }
    }
}