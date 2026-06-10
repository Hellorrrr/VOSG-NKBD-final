using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VOSG_NKBD.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        [Required]
        public int ConfirmationID { get; set; }

        [Required]
        public string VOSG_NKBDId { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Payment Amount")]
        public decimal PaymentAmount { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string PaymentStatus { get; set; } = "Paid";

        [ForeignKey("ConfirmationID")]
        public Confirmation? Confirmation { get; set; }

        [ForeignKey("VOSG_NKBDId")]
        public VOSG_NKBDUser? User { get; set; }
    }
}
