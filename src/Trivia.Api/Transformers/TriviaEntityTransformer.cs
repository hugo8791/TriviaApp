using Trivia.Api.Models;
using Trivia.Shared.Models;

namespace Trivia.Api.Transformers;

public static class TriviaEntityTransformer
{
    public static QuizQuestion ToQuizQuestion(TriviaQuestionEntity entity) =>
        new(
            entity.Id,
            entity.Question,
            entity.Category,
            entity.Difficulty,
            entity.Answers
                .Select(a => new QuizAnswer(a.Id, a.Text))
                .OrderBy(_ => Random.Shared.Next())
                .ToList()
        );
}