using System.ComponentModel.DataAnnotations;

namespace BabySiimDiscordBot.Models
{
    public class FredyConstant
    {
        [Key]
        public string Name { get; set; }

        [Required]
        [MaxLength(256)]
        public double Value { get; set; }
    }
}
