using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }
    [Required]
    public int OrderId { get; set; }
    [Required, StringLength(50)]
    public string PaymentMethod { get; set; } = null!;
    
    public DateTime? PaymentDate { get; set; } = DateTime.Now;

    [Required]
    [Column(TypeName ="decimal(10,2)")]
    public decimal AmountPaid { get; set; }
    [Required,StringLength(50)]
    public string? PaymentStatus { get; set; }
    [ForeignKey("OrderID")]
    public virtual Order Order { get; set; } = null!;
}
