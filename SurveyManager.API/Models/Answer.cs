using System;

namespace SurveyManager.API.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int UserId { get; set; }
        public AnswerUser User { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}