using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOSG_NKBD.Models
{
    public class Place
    {
        [Key]
        public int PlaceID { get; set; }

        [Required]
        [Display(Name = "Place / Section Name")]
        public string PlaceName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Price (NZD)")]
        public decimal Price { get; set; }

        [Required]
        public int LocationsID { get; set; }

        [ForeignKey("LocationsID")]
        public Location? Location { get; set; }

        public ICollection<Confirmation>? Confirmations { get; set; }
    }
}