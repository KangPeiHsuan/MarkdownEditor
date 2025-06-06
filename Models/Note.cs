using System.ComponentModel.DataAnnotations;

namespace MarkdownEditor.Models
{
    public class Note
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public int CategoryId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        
        // Navigation property
        public virtual Category Category { get; set; } = null!;
    }
} 