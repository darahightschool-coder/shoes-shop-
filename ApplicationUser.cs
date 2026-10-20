using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ShoeShop.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
