using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Token : BaseEntity
{
    [Required]
    public required int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public required User User { get; set; }

    [Required]
    [MaxLength(500)]
    public required string AccessToken { get; set; }

    [Required]
    [MaxLength(500)]
    public required string RefreshToken { get; set; }

    [Required]
    public required long ExpiresIn { get; set; }

    [Required]
    [MaxLength(50)]
    public required string TokenType { get; set; }
}
