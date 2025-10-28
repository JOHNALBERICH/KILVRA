using System.ComponentModel.DataAnnotations;

namespace KILVRA.Area.Admin.Models
{
    public class Coupon
    {
    [Key]
    public int CouponId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!; // Example: SAVE10, WELCOME20

    [Range(0, 100)]
    public decimal DiscountPercent { get; set; } // % discount (like 10, 20)

    public DateTime? ExpiryDate { get; set; } // optional

    public bool IsActive { get; set; } = true;
  }
}
