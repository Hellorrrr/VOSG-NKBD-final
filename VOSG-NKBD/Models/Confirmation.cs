using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOSG_NKBD.Models
{
    public class Confirmation
    {
        [Key]
        public int ConfirmationID { get; set; }

        [Required]
        public string MemberId { get; set; } = string.Empty;

        [Required]
        public int PlaceID { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Confirmation Date")]
        public DateTime ConfirmationDate { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required, DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total Price")]
        public decimal TotalPrice { get; set; }

        [ForeignKey("MemberId")]
        public VOSG_NKBDUser? User { get; set; }

        [ForeignKey("PlaceID")]
        public Place? Place { get; set; }

        public Payment? Payment { get; set; }
    }
}