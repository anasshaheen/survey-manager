using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SurveyManager.API.Models
{
    public class Survey
    {
        public Survey()
        {
            ShareUsers = new List<SurveyShareUser>();
            Questions = new List<Question>();
            Pages = new List<Page>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsDraft { get; set; }
        public PrivacyTypes PrivacyType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public SurveyShare Share { get; set; }
        public List<Question> Questions { get; set; }
        public List<SurveyShareUser> ShareUsers { get; set; }
        public List<Page> Pages { get; internal set; }
        [NotMapped]
        public int? NumberOfResponses { get; set; }
    }
}