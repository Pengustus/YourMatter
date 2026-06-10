using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace YourMatter.Models.Profile
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Display Name is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Display Name must be between 3 and 50 characters.")]
        public string DisplayName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string? Bio { get; set; }

        [StringLength(100, ErrorMessage = "Location cannot exceed 100 characters.")]
        public string? Location { get; set; }

        [Display(Name = "Profile Picture URL")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Upload Profile Picture")]
        public IFormFile? ProfilePictureFile { get; set; }
    }
}
