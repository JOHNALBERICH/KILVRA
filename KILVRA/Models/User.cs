using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KILVRA.Models;

public partial class User
{
    [Key]
    public int UserId { get; set; }
    [Required, StringLength(100)]
    public string FullName { get; set; } = null!;
    [Required, StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = null!;
    [StringLength(15)]
    public string? Phone { get; set; }
    [Required, StringLength(255)]
    public string? Address { get; set; }
    [Required, StringLength(50)]
    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; } =DateTime.Now;

    [StringLength(100)]
    public string? Provider { get; set; }
    public virtual Admin? Admin { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}
