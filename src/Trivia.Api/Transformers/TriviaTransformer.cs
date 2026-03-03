using Trivia.Api.Models;

namespace Trivia.Api.Transformers;

public static class TriviaTransformer
{
    public static TriviaQuestionEntity ToEntity(TriviaQuestion resource) =>
        new()
        {
            Question = resource.Question,
            Category = resource.Category,
            Difficulty = resource.Difficulty,
            FetchedAt = DateTimeOffset.UtcNow,
            Answers = ToAnswers(resource.CorrectAnswer, resource.IncorrectAnswers)
        };

    private static List<TriviaAnswerEntity> ToAnswers(string correctAnswer, List<string> incorrectAnswers)
    {
        var answers = incorrectAnswers
            .Select(a => ToAnswer(a, isCorrect: false))
            .ToList();
        answers.Add(ToAnswer(correctAnswer, isCorrect: true));
        return answers;
    }

    private static TriviaAnswerEntity ToAnswer(string text, bool isCorrect) =>
        new() { Text = text, IsCorrect = isCorrect };
}
