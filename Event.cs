
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("EVENT")]
public class Event
{
    [Key]
    public int EVENT_ID { get; set; }

    [Required]
    public int ORGANISER_ID { get; set; }

    [Required, MaxLength(100)]
    public string EVENT_NAME { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string DESCRIPTION { get; set; } = string.Empty;

    [Required]
    public DateTime EVENT_DATE { get; set; }

    [Required, MaxLength(150)]
    public string LOCATION { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string STATUS { get; set; } = "Upcoming";

    [Required, MaxLength(20)]
    public string EVENT_TYPE { get; set; } = "Run";

    [Required]
    [Column(TypeName = "decimal(6,2)")]
    public decimal DISTANCE { get; set; }

    [ForeignKey(nameof(ORGANISER_ID))]
    public Organiser? Organiser { get; set; }
}
