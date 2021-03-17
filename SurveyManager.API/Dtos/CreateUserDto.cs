using SurveyManager.API.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SurveyManager.API.Dtos
{
    public class CreateUserDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        [Required]
        [MaxLength(60)]
        public string Password { get; set; }

        public string CompanyName { get; set; }
    }

    public class UpdateUserDto
    {
        [MaxLength(100)]
        public string FirstName { get; set; }
        [MaxLength(100)]
        public string LastName { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }
        [Required]
        [MaxLength(60)]
        public string Password { get; set; }
    }

    public class CreateSurveyDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }
    }

    public class PageDto
    {
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        public int? Order { get; set; }
    }

    public class UpdateSurveyDto
    {
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public bool? IsDraft { get; set; }
    }

    public class SurveyShareDto
    {
        public SurveyShareDto()
        {
            Emails = new List<string>();
        }

        public PrivacyTypes PrivacyType { get; set; }
        public List<string> Emails { get; set; }
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; }
    }

    public class SaveAnswersDto
    {
        public List<AnswerDto> Answers { get; set; }

        public string ExtraDetails { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UID { get; set; }
    }

    public class QuestionDto
    {
        public int? Id { get; set; }

        public int TypeId { get; set; }

        public bool IsMain { get; set; }

        public int Order { get; set; }

        public bool IsActive { get; set; }

        public string AnswerSchema { get; set; }

        public string Title { get; set; }
    }

    public class QuestionsDto
    {
        public List<QuestionDto> Questions { get; set; }
    }
}