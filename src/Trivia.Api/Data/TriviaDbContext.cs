using Microsoft.EntityFrameworkCore;
using Trivia.Api.Models;

namespace Trivia.Api.Data;

public class TriviaDbContext(DbContextOptions<TriviaDbContext> options) : DbContext(options)
{
    public DbSet<TriviaQuestionEntity> Questions { get; set; }
    public DbSet<TriviaAnswerEntity> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TriviaQuestionEntity>()
            .HasMany(q => q.Answers)
            .WithOne()
            .HasForeignKey(a => a.TriviaQuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TriviaQuestionEntity>()
            .HasIndex(q => q.Question)
            .IsUnique();
    }
}