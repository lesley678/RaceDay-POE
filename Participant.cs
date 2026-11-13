using
    System.ComponentModel.DataAnnotations;
using
    System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

[Table("PARTICIPANT")]
public class Participant
{
    [Key] public int PARTICIPANT_ID { get; set; }

    [Required]
    public int USER_ID { get; set; }

    [Required]
    public DateTime DATE_OF_BIRTH { get; set; } 

    [Required, MaxLength(20)]
    public string GENDER { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string EMERGENCY_CONTACT { get; set; } = string.Empty;

    [ForeignKey(nameof(USER_ID))]
    public User? User { get; set; }

    

}