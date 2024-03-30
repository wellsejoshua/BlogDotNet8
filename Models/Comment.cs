using BlogDotNet8.Enums;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace BlogDotNet8.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? BlogUserId { get; set; }
        public string? ModeratorId { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and no more than {1}} chracters long.", MinimumLength = 2)]
        [Display(Name = "Comment")]
        public string? Body { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Moderated Date")]
        public DateTime? Moderated { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Deleted Date")]
        public DateTime? Deleted { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and no more than {1}} chracters long.", MinimumLength = 2)]
        [Display(Name = "Moderated Comment")]
        public string? ModeratedBody { get; set; }

        public ModerationType ModerationType { get; set; }

        //Navigation Properties
        public virtual Post? Post { get; set; }
        public virtual BlogUser? BlogUser { get; set; }
        public virtual BlogUser? Moderator { get; set; }
    }
}
