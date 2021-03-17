using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyManager.API.Data;
using SurveyManager.API.Dtos;
using SurveyManager.API.Helpers;
using SurveyManager.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SurveyManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SurveysController : ControllerBase
    {
        private readonly SurveyManagerContext _context;
        private readonly IMapper _mapper;

        public SurveysController(SurveyManagerContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateSurveyDto dto)
        {
            var survey = new Survey
            {
                Title = dto.Title,
                Description = dto.Description,
                IsDraft = false,
                CreatedAt = DateTime.Now,
                UserId = User.UserId()
            };
            await _context.Surveys.AddAsync(survey);
            await _context.SaveChangesAsync();

            return Created(nameof(Create), new { id = survey.Id });
        }

        [AllowAnonymous]
        [HttpGet("Get")]
        public async Task<ActionResult> Get([FromQuery] string uid = null, [FromQuery] string email = null, [FromQuery] int? id = null, int? pageId = null)
        {
            var query = _context.Surveys.AsQueryable();
            if (User?.Identity == null || !User.Identity.IsAuthenticated)
            {
                query = query.Where(e => !e.IsDraft);
            }

            if (User.Identity.IsAuthenticated)
            {
                if (id == null)
                {
                    if (string.IsNullOrWhiteSpace(uid))
                    {
                        return BadRequest(new { errors = new List<string> { "UID must be valid!" } });
                    }

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        query = query.Include(e => e.Share)
                            .Where(e => e.Share.UniqueId.Equals(uid));
                    }
                    else
                    {
                        var shareUser = await _context.SurveyShareUsers
                            .SingleOrDefaultAsync(e => e.Email.ToLower().Equals(email.ToLower()) &&
                                                       e.UniqueId.Equals(uid));
                        if (shareUser == null)
                        {
                            return NotFound();
                        }

                        query = query.Include(e => e.ShareUsers)
                            .Where(e => e.Id == shareUser.SurveyId);
                    }
                }
                else
                {
                    query = query.Where(e => e.Id == id);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(uid))
                {
                    return BadRequest(new { errors = new List<string> { "UID must be valid!" } });
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    query = query.Include(e => e.Share)
                        .Where(e => e.Share.UniqueId.Equals(uid));
                }
                else
                {
                    var shareUser = await _context.SurveyShareUsers
                        .SingleOrDefaultAsync(e => e.Email.ToLower().Equals(email.ToLower()) &&
                                                   e.UniqueId.Equals(uid));
                    if (shareUser == null)
                    {
                        return NotFound();
                    }

                    query = query.Include(e => e.ShareUsers)
                        .Where(e => e.Id == shareUser.SurveyId);
                }
            }

            if (!pageId.HasValue)
            {
                query = query.Include(e => e.Pages)
                    .ThenInclude(e => e.Questions);
            }
            else
            {
                query = query.Include(e => e.Pages);
            }

            var survey = await query
                .Include(e => e.Share)
                .Include(e => e.ShareUsers)
                .Include(e => e.User)
                .SingleOrDefaultAsync();
            if (survey == null)
            {
                return NotFound();
            }

            if (pageId.HasValue && survey.Questions.Any())
            {
                var page = survey.Pages.SingleOrDefault(e => e.Id == pageId);
                if (page != null)
                {
                    page.Questions = await _context.Questions
                        .Where(e => e.PageId == pageId && e.SurveyId == survey.Id)
                        .ToListAsync();
                }
            }

            survey.Questions = null;
            survey.NumberOfResponses = await _context.AnswerUsers.CountAsync(e => e.SurveyId == survey.Id);

            return Ok(survey);
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult> GetAll()
        {
            var surveys = await _context.Surveys
                .Include(e => e.Share)
                .Include(e => e.ShareUsers)
                .Where(e => e.UserId == User.UserId())
                .ToListAsync();
            foreach (var survey in surveys)
            {
                survey.NumberOfResponses = await _context.AnswerUsers.CountAsync(e => e.SurveyId == survey.Id);
            }

            return Ok(surveys);
        }

        [HttpPost("{id}/Share")]
        public async Task<ActionResult> Share(int id, SurveyShareDto dto)
        {
            var survey = await _context.Surveys.SingleOrDefaultAsync(e => e.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            var errors = new List<string>();
            var tokens = new Dictionary<string, string>();
            if (dto.PrivacyType == PrivacyTypes.Private)
            {
                if (dto.Emails.Any())
                {
                    foreach (var email in dto.Emails)
                    {
                        var surveyShare = await _context.SurveyShareUsers.SingleOrDefaultAsync(e =>
                            e.SurveyId == id && e.Email.ToLower().Equals(email.ToLower()));
                        if (surveyShare != null)
                        {
                            errors.Add($"Survey already shared, use {surveyShare.UniqueId} to access survey for email {surveyShare.Email}.");

                            continue;
                        }

                        var shareUser = new SurveyShareUser
                        {
                            SurveyId = survey.Id,
                            CreatedAt = DateTime.Now,
                            Email = email,
                            UniqueId = Guid.NewGuid().ToString(),
                            UserId = _context.Users.SingleOrDefault(x => x.Email.ToLower().Equals(email.ToLower()))?.Id
                        };
                        await _context.SurveyShareUsers.AddAsync(shareUser);

                        tokens.Add(email, shareUser.UniqueId);
                    }
                }
            }
            else
            {
                var surveyShare = await _context.SurveyShares
                    .SingleOrDefaultAsync(e => e.SurveyId == id);
                if (surveyShare != null)
                {
                    survey.PrivacyType = dto.PrivacyType;
                    _context.Surveys.Update(survey);
                    await _context.SaveChangesAsync();

                    tokens.Add("ALL", surveyShare.UniqueId);

                    return Ok(new { tokens });
                }

                var share = new SurveyShare
                {
                    SurveyId = survey.Id,
                    CreatedAt = DateTime.Now,
                    UniqueId = Guid.NewGuid().ToString(),
                };
                await _context.SurveyShares.AddAsync(share);
                tokens.Add("ALL", share.UniqueId);
            }

            survey.PrivacyType = dto.PrivacyType;
            _context.Surveys.Update(survey);
            await _context.SaveChangesAsync();

            if (errors.Any())
            {
                return BadRequest(new { errors, tokens });
            }

            return Ok(new { tokens });
        }

        [HttpPost("results")]
        [AllowAnonymous]
        public async Task<ActionResult> SaveAnswers(SaveAnswersDto dto)
        {
            var query = _context.Surveys.AsQueryable();
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                query = query.Include(e => e.Share)
                    .Where(e => e.Share.UniqueId.Equals(dto.UID));
            }
            else
            {
                var shareUser = await _context.SurveyShareUsers
                    .SingleOrDefaultAsync(e => e.Email.ToLower().Equals(dto.Email.ToLower()) &&
                                               e.UniqueId.Equals(dto.UID));
                if (shareUser == null)
                {
                    return NotFound();
                }

                query = query.Include(e => e.ShareUsers)
                    .Where(e => e.Id == shareUser.SurveyId);
            }

            var survey = await query.SingleOrDefaultAsync();
            if (survey == null)
            {
                return NotFound();
            }

            if (!dto.Answers.Any())
            {
                return BadRequest(new { errors = new List<string> { "There's no answers in the request body." } });
            }

            var answerUser = new AnswerUser
            {
                Email = dto.Email,
                UserId = User.OptionalUserId(),
                SurveyId = survey.Id,
                ExtraDetails = dto.ExtraDetails,
                Answers = dto.Answers.Select(e => new Answer
                {
                    QuestionId = e.QuestionId,
                    Text = e.Text,
                    CreatedAt = DateTime.Now
                }).ToList(),
                CreatedAt = DateTime.Now
            };
            await _context.AnswerUsers.AddAsync(answerUser);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateSurveyDto dto)
        {
            var survey = await _context.Surveys.SingleOrDefaultAsync(e => e.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            if (survey.UserId != User.UserId())
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                survey.Title = dto.Title;
            }
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                survey.Description = dto.Description;
            }

            if (dto.IsDraft.HasValue)
            {
                survey.IsDraft = dto.IsDraft.Value;
            }

            _context.Surveys.Update(survey);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var survey = await _context.Surveys
                .Include(e => e.Share)
                .Include(e => e.ShareUsers)
                .Include(e => e.User)
                .Include(e => e.Pages)
                    .ThenInclude(e => e.Questions)
                .SingleOrDefaultAsync(e => e.Id == id);
            if (survey == null)
            {
                return NotFound();
            }

            if (survey.UserId != User.UserId())
            {
                return Unauthorized();
            }

            _context.Surveys.Remove(survey);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{surveyId}/questions/{pageId}")]
        public async Task<ActionResult> SaveQuestions(int surveyId, int pageId, QuestionsDto dto)
        {
            var survey = await _context.Surveys.SingleOrDefaultAsync(e => e.Id == surveyId);
            if (survey == null)
            {
                return NotFound();
            }

            if (survey.UserId != User.UserId())
            {
                return Unauthorized();
            }

            var dbQuestions = await _context.Questions
                .Where(e => e.SurveyId == surveyId && e.PageId == pageId)
                .ToListAsync();
            if (dbQuestions.Any())
            {
                _context.Questions.RemoveRange(dbQuestions);
                await _context.SaveChangesAsync();
            }

            var questions = dto.Questions.Where(e => !e.Id.HasValue)
                .Select(e => new Question
                {
                    Order = e.Order,
                    AnswerSchema = e.AnswerSchema,
                    Title = e.Title,
                    SurveyId = surveyId,
                    CreatedAt = DateTime.Now,
                    IsMain = e.IsMain,
                    IsActive = e.IsActive,
                    PageId = pageId,
                    TypeId = e.TypeId,
                });
            await _context.Questions.AddRangeAsync(questions);
            await _context.SaveChangesAsync();

            //var ids = dto.Questions
            //    .Where(e => e.Id.HasValue)
            //    .Select(e => e.Id.Value)
            //    .ToList();
            //if (ids.Any())
            //{
            //    var questionsToUpdate = await _context.Questions.Where(e => ids.Contains(e.Id))
            //        .ToListAsync();
            //    foreach (var question in questionsToUpdate.ToList())
            //    {
            //        var questionDto = dto.Questions.First(e => e.Id == question.Id);

            //        question.Order = questionDto.Order;
            //        question.AnswerSchema = questionDto.AnswerSchema;
            //        question.Title = questionDto.Title;
            //        question.UpdatedAt = DateTime.Now;
            //        question.IsMain = questionDto.IsMain;
            //        question.IsActive = questionDto.IsActive;
            //        question.PageId = questionDto.PageId;
            //        question.TypeId = questionDto.TypeId;

            //        _context.Questions.Update(question);
            //    }

            //    await _context.SaveChangesAsync();
            //}

            return Ok();
        }

        [HttpGet("{surveyId}/answers")]
        public async Task<ActionResult> GetAnswers(int surveyId)
        {
            var survey = await _context.Surveys
                .Include(e => e.Questions)
                .AsNoTracking()
                .SingleOrDefaultAsync(e => e.Id == surveyId);
            if (survey.UserId != User.UserId())
            {
                return Unauthorized();
            }

            var answerUsers = await _context.AnswerUsers
                .Include(e => e.Answers)
                .ThenInclude(e => e.Question)
                .Include(e => e.User)
                .Where(e => e.SurveyId == surveyId)
                .OrderBy(e => e.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            if (answerUsers == null || !answerUsers.Any())
            {
                return NotFound();
            }

            if (!survey.Questions.Any())
            {
                return BadRequest(new { errors = new List<string> { "Survey contains no questions to export." } });
            }

            return Ok(_mapper.Map<List<AnswerUserViewModel>>(answerUsers));
        }

        [HttpPost("{surveyId}/Pages")]
        public async Task<ActionResult> CreatePage(int surveyId, PageDto dto)
        {
            var page = new Page
            {
                Title = dto.Title,
                UserId = User.UserId(),
                SurveyId = surveyId,
                Order = dto.Order ?? 0
            };
            await _context.Pages.AddAsync(page);
            await _context.SaveChangesAsync();

            return Created(nameof(Create), new { id = page.Id });
        }

        [HttpPut("{surveyId}/Pages/{pageId}")]
        public async Task<ActionResult> UpdatePage(int surveyId, int pageId, PageDto dto)
        {
            var page = await _context.Pages
                .SingleOrDefaultAsync(e => e.SurveyId == surveyId && e.Id == pageId);
            if (page == null)
            {
                return NotFound();
            }

            if (page.UserId != User.UserId())
            {
                return Unauthorized();
            }

            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                page.Title = dto.Title;
            }

            if (dto.Order.HasValue)
            {
                page.Order = dto.Order.Value;
            }

            _context.Pages.Update(page);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{surveyId}/Pages/{pageId}")]
        public async Task<ActionResult> DeletePage(int surveyId, int pageId)
        {
            var page = await _context.Pages
                .Include(e => e.Questions)
                .SingleOrDefaultAsync(e => e.SurveyId == surveyId && e.Id == pageId);
            if (page == null)
            {
                return NotFound();
            }

            if (page.UserId != User.UserId())
            {
                return Unauthorized();
            }

            _context.Questions.RemoveRange(page.Questions);
            await _context.SaveChangesAsync();

            _context.Pages.Remove(page);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{surveyId}/Pages/{pageId}")]
        public async Task<ActionResult> GetPage(int surveyId, int pageId)
        {
            var page = await _context.Pages
                .Include(e => e.Questions)
                .SingleOrDefaultAsync(e => e.SurveyId == surveyId && e.Id == pageId);
            if (page == null)
            {
                return NotFound();
            }

            return Ok(page);
        }

        [HttpGet("{surveyId}/Pages")]
        public async Task<ActionResult> GetPages(int surveyId)
        {
            var pages = await _context.Pages
                .Include(e => e.Questions)
                .Where(e => e.SurveyId == surveyId)
                .OrderBy(e => e.Order)
                .ToListAsync();

            return Ok(pages);
        }
    }
}