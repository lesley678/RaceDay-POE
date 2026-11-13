
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("CATEGORY")]
public class Category
{
    [Key]
    public int CATEGORY_ID { get; set; }

    [Required]
    public int EVENT_ID { get; set; }

    [Required, MaxLength(100)]
    public string CATEGORY_NAME { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")]
    public decimal DISTANCE { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal ENTRY_FEE { get; set; } = 0;

    [ForeignKey(nameof(EVENT_ID))]
    public Event? Event { get; set; }
}

