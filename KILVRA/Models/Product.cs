using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }
    
    public int ShopId { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = null!;
    [StringLength(500)]
    public string? Description { get; set; }
    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }
    [Required]
    public int? Quantity { get; set; }
    [StringLength(50)]
    public string? Category { get; set; }
    [StringLength(20)]
    public string? Size { get; set; }

    public string? ImageUrl { get; set; }
    

    public DateTime? CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    [ForeignKey("ShopId")]
    public virtual Shop? Shop { get; set; } 
}
