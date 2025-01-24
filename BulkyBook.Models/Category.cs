using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100, ErrorMessage = "Category Name should not greater than 100 characters")]
        [Required]
        [DisplayName("Category Name")]
        public required string Name { get; set; }
        [Range(1, 100, ErrorMessage = "Display Order should between 1 - 100")]
        [DisplayName("Display Order")]
        public int DisplayOrder { get; set; }
    }
}
