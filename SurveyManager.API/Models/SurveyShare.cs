using System;

namespace SurveyManager.API.Models
{
    public class SurveyShare
    {
        public int SurveyId { get; set; }
        public Survey Survey { get; set; }

        public string UniqueId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}