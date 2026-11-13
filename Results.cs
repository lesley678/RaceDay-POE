
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("RESULT")]
public class Result
{
    [Key]
    public int RESULT_ID { get; set; }

    [Required]
    public int ENROLMENT_ID { get; set; }

    public TimeSpan? FINISH_TIME { get; set; }

    public int? POSITION { get; set; }

    [Required, MaxLength(20)]
    public string RESULT_STATUS { get; set; } = "Pending";

    [ForeignKey(nameof(ENROLMENT_ID))]
    public Enrolment? Enrolment { get; set; }
}
