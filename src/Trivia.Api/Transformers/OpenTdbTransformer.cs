using System.Net;
using Trivia.Api.Models;

namespace Trivia.Api.Transformers;

public static class OpenTdbTransformer
{
    public static TriviaQuestionEntity ToEntity(TriviaQuestion resource) =>
        new()
        {
            Question = WebUtility.HtmlDecode(resource.Question),
            Category = WebUtility.HtmlDecode(resource.Category),
            Difficulty = char.ToUpper(resource.Difficulty[0]) + resource.Difficulty[1..],
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
        new() { Text = WebUtility.HtmlDecode(text), IsCorrect = isCorrect };
}
