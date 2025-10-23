using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class Order
{
    [Key]
    public int OrderId { get; set; }
    [Required]
    public int UserId { get; set; }

    public DateTime? OrderDate { get; set; }=DateTime.Now;

    [Required, StringLength(50)]
    public string? Status { get; set; }
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }
    [StringLength(250)]
    public string? ShippingAddress { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Payment? Payment { get; set; }
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
   

}
