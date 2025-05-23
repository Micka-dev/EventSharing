using System.ComponentModel.DataAnnotations;

namespace EventSharing.ViewModels
{
    public class EventViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required" )]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot be longer than 100 characters.")]

        public string? Description { get; set; }
        [Required(ErrorMessage = "StartDate is required")]

        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "EndDate is required")]

        public DateTime? EndDate { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public List<CategoryViewModel>? CategoriesVm { get; set; }

    }
}
