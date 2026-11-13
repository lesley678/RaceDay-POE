
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("ENROLMENT")]
public class Enrolment
{
    [Key]
    public int ENROLMENT_ID { get; set; }

    [Required]
    public int PARTICIPANT_ID { get; set; }

    [Required]
    public int CATEGORY_ID { get; set; }

    public DateTime ENROLMENT_DATE { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string STATUS { get; set; } = "Active";

    [ForeignKey(nameof(PARTICIPANT_ID))]
    public Participant? Participant { get; set; }

    [ForeignKey(nameof(CATEGORY_ID))]
    public Category? Category { get; set; }
}
