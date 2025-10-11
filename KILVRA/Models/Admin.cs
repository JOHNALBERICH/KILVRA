using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class Admin
{
    [Key]
    public int AdminId { get; set; }
    [Required]
    public int UserId { get; set; }
    [StringLength(100)]
    public string? Position { get; set; }

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
