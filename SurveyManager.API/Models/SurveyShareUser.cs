using System;

namespace SurveyManager.API.Models
{
    public class SurveyShareUser
    {
        public int Id { get; set; }
        
        public int SurveyId { get; set; }
        public Survey Survey { get; set; }

        public string UniqueId { get; set; }

        public string Email { get; set; }
        
        public int? UserId { get; set; }
        public User User { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}