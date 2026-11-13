
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("ORGANISER")]
public class Organiser
{
    [Key]
    public int ORGANISER_ID { get; set; }

    [Required]
    public int USER_ID { get; set; }

    [Required]
    [MaxLength(100)]
    public string ORGANISATION_NAME { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string CONTACT_NUMBER { get; set; } = string.Empty;

    [ForeignKey(nameof(USER_ID))]
    public User? User { get; set; }
}
