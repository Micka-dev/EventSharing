using Microsoft.EntityFrameworkCore;

namespace EventSharing.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Category? Category { get; set; }
        public User? Creator { get; set; }
        public string? CreatorId { get; set; }
        public int Capacity { get; set; }
        /// ///////
        public List<User>? Participants { get; set; } = new List<User>();//////////

    }
}
