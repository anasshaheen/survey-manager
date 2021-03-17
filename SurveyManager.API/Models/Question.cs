using System;
using System.Collections.Generic;

namespace SurveyManager.API.Models
{
    public class Question
    {
        public Question()
        {
            Answers = new List<Answer>();
        }

        public int Id { get; set; }
        public Survey Survey { get; set; }
        public int SurveyId { get; set; }
        public QuestionTypes Type { get; set; }
        public int TypeId { get; set; }
        public bool IsMain { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public string AnswerSchema { get; set; }
        public int? PageId { get; set; }
        public Page Page { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<Answer> Answers { get; set; }
    }
}