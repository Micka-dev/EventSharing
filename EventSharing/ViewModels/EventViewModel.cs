using EventSharing.Models;
using Microsoft.EntityFrameworkCore;
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
        [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "StartDate is required")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "EndDate is required")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "CategoryName is required")]
        public int? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public List<CategoryViewModel>? CategoriesVm { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        //Liste des noms des participants (pour l'affichage dans la vue)
        public List<User>? Participants { get; set; } = new List<User>();

        public List<string>? ParticipantDetails => Participants?.Select(p => $"{p.Name} ( {p.Email} | {p.PhoneNumber} )").ToList();

        public User? Creator { get; set; }
        public string? CreatorId { get; set; }


    }
}
