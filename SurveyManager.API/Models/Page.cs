using System;
using System.Collections.Generic;

namespace SurveyManager.API.Models
{
    public class Page
    {
        public Page()
        {
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        public int Id { get; set; }

        public string Title { get; set; }

        public Survey Survey { get; set; }
        public int SurveyId { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int Order { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<Question> Questions { get; set; }
    }
}