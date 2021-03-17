using System;
using System.Collections.Generic;

namespace SurveyManager.API.Models
{
    public class AnswerUser
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string ExtraDetails { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public int SurveyId { get; set; }
        public Survey Survey { get; set; }

        public List<Answer> Answers { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AnswerUserViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string ExtraDetails { get; set; }

        public int? UserId { get; set; }
        public UserViewModel User { get; set; }

        public List<AnswerViewModel> Answers { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AnswerViewModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public QuestionViewModel Question { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class QuestionViewModel
    {
        public int Id { get; set; }
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
    }

    public class UserViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}