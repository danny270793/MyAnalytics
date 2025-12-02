using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Backend.Models;

[Index(nameof(Username), IsUnique = true)]
public class User : BaseEntity
{
    [Required]
    [MaxLength(255)]
    [MinLength(4)]
    public required string Username { get; set; }

    [Required]
    public required string Password { get; set; }
}
