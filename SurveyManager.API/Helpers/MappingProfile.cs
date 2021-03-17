using AutoMapper;
using SurveyManager.API.Models;

namespace SurveyManager.API.Helpers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<AnswerUser, AnswerUserViewModel>().ReverseMap();
            CreateMap<Answer, AnswerViewModel>().ReverseMap();
            CreateMap<Question, QuestionViewModel>().ReverseMap();
            CreateMap<User, UserViewModel>().ReverseMap();
        }
    }
}
