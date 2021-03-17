using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SurveyManager.API.Data;
using SurveyManager.API.Dtos;
using SurveyManager.API.Models;
using SurveyManager.API.Options;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace SurveyManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly SurveyManagerContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly JwtConfig _jwtConfig;

        public AuthController(SurveyManagerContext context,
            SignInManager<User> signInManager,
            IOptions<JwtConfig> options)
        {
            _context = context;
            _signInManager = signInManager;
            _jwtConfig = options.Value;
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .SingleOrDefaultAsync(x => x.UserName.ToLower().Equals(dto.Email.ToLower()));
            if (user == null)
            {
                return NotFound();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
            {
                return BadRequest(new {errors = new List<string> {"Email or password is wrong!"}});
            }

            return Ok(new
            {
                Token = GenerateAccessToken(user),
                User = new
                {
                    user.FirstName,
                    user.LastName,
                    user.Id,
                    user.Email
                }
            });
        }
        
        private object GenerateAccessToken(User user)
        {
            var claims = new List<Claim>(){
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? ""),
                new Claim("UserId", "" + user.Id ?? ""),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddMonths(1);

            var accessToken = new JwtSecurityToken(_jwtConfig.Issuer,
                _jwtConfig.Site,
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(accessToken),
                ExpiredAt = expires
            };
        }
    }
}