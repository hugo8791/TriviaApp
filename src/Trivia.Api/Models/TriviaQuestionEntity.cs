using System.ComponentModel.DataAnnotations;

namespace Trivia.Api.Models;

public class TriviaQuestionEntity
{
    public int Id { get; set; }

    [MaxLength(500)]
    public required string Question { get; set; }

    [MaxLength(100)]
    public required string Category { get; set; }

    [MaxLength(20)]
    public required string Difficulty { get; set; }

    public List<TriviaAnswerEntity> Answers { get; set; } = [];

    public DateTimeOffset FetchedAt { get; set; }
}