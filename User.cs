using
    System.ComponentModel.DataAnnotations;
using
    System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;

[Table("USER")]
public class User
{
    [Key] public int USER_ID { get;set; }

    [Required,MaxLength(50)]
    public string FIRSTNAME { get;set; }=string.Empty;

    [Required,MaxLength(50)]
    public string LASTNAME { get;set; } = string.Empty;

    [Required,MaxLength(100)]
    public string EMAIL { get;set; } =string.Empty;

    [Required,MaxLength(225)]
    public string PASSWORD_HASH { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ROLE { get; set; } = "Participant";

    public DateTime CREATED_AT { get; set; } = DateTime.UtcNow;

}