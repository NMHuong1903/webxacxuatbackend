using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace WebAPI.Data
{
    public class DataDbContext: DbContext
    {
        public DataDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }
        public DbSet<StudentExam> StudentExams { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<AdviceRules> AdviceRules { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
