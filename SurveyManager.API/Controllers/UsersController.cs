using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class UsersController : ControllerBase
    {
        private readonly SurveyManagerContext _context;
        private readonly UserManager<User> _userManger;

        public UsersController(SurveyManagerContext context,
                               UserManager<User> userManger)
        {
            _context = context;
            _userManger = userManger;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Create(CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(x => x.Email.ToLower().Equals(dto.Email.ToLower())))
            {
                return BadRequest(new { errors = new List<string> { "Email already exists." } });
            }

            var user = new User
            {
                UserName = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                CompanyName = dto.CompanyName
            };
            var result = await _userManger.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Created(nameof(Create), new { id = user.Id });
        }

        [HttpPut]
        public async Task<ActionResult> Update(UpdateUserDto dto)
        {
            var userId = User.UserId();
            var user = _context.Users.SingleOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
            {
                user.FirstName = dto.FirstName;
            }

            if (!string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.LastName = dto.LastName;
            }
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Profile(int? id = null)
        {
            var userId = User.UserId();
            if (userId == null && id == null)
            {
                return BadRequest(new
                { errors = new List<string> { "User should be authenticated or user id should be provided" } });
            }

            var user = await _context.Users.SingleOrDefaultAsync(x =>
                userId == null ? x.Id == id : x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                user.FirstName,
                user.LastName,
                user.Id,
                user.Email
            });
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string id)
        {
            var userId = User.UserId();
            if (userId == null && id == null)
            {
                return BadRequest(new
                { errors = new List<string> { "User should be authenticated or user id should be provided" } });
            }

            var user = _context.Users.SingleOrDefault(x =>
                x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}