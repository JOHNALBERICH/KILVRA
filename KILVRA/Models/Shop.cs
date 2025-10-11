using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class Shop
{
    [Key]
    public int ShopId { get; set; }
    [Required]
    public int AdminId { get; set; }
    [Required, StringLength(100)]
    public string ShopName { get; set; } = null!;
    [StringLength(500)]
    public string? Description { get; set; }
    [StringLength(255)]
    public string? Location { get; set; }

    public DateTime? CreatedAt { get; set; } = DateTime.Now;
    [ForeignKey("AdminId")]
    public virtual Admin? Admin { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
