using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SurveyManager.API.Models;

namespace SurveyManager.API.Data
{
    public class SurveyManagerContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public SurveyManagerContext(DbContextOptions<SurveyManagerContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SurveyShare>()
                .HasKey(e => e.SurveyId);

            builder.Entity<SurveyShare>()
                .HasOne(e => e.Survey)
                .WithOne(e => e.Share)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Survey>()
                .HasMany(e => e.Questions)
                .WithOne(e => e.Survey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Survey>()
                .HasMany(e => e.ShareUsers)
                .WithOne(e => e.Survey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Survey>()
                .HasMany(e => e.Pages)
                .WithOne(e => e.Survey)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Question>()
                .HasMany(e => e.Answers)
                .WithOne(e => e.Question)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Answer> Answers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Survey> Surveys { get; set; }
        public DbSet<SurveyShare> SurveyShares { get; set; }
        public DbSet<SurveyShareUser> SurveyShareUsers { get; set; }
        public DbSet<AnswerUser> AnswerUsers { get; set; }
    }
}