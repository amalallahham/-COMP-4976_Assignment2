using System;
using System.ComponentModel.DataAnnotations;

namespace ObituaryApplication.Models
{
    public class Obituary
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "Date of death is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Death")]
        public DateTime DOD { get; set; }

        [Required(ErrorMessage = "Biography is required.")]
        [MinLength(10, ErrorMessage = "Biography must be at least 10 characters long.")]
        [Display(Name = "Biography / Tribute")]
        public string Biography { get; set; } = string.Empty;

        public string? PhotoPath { get; set; }

        public string CreatorId { get; set; } = string.Empty;
    }
}
