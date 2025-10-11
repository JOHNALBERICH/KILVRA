using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class OrderDetail
{
    [Key]
    public int OrderDetailId { get; set; }
    [Required]
    public int OrderId { get; set; }
    [Required]
    public int ProductId { get; set; }
    [Required]
    public int Quantity { get; set; }
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal UnitPrice { get; set; }
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public decimal? SubTotal { get; set; }
    [ForeignKey("OrderId")]
    public virtual Order Order { get; set; } = null!;
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}
