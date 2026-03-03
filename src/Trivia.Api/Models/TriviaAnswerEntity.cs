using System.ComponentModel.DataAnnotations;

namespace Trivia.Api.Models;

public class TriviaAnswerEntity
{
    public int Id { get; set; }

    [MaxLength(500)]
    public required string Text { get; set; }

    public bool IsCorrect { get; set; }

    public int TriviaQuestionId { get; set; }
}