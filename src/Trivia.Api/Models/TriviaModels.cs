using System.Text.Json.Serialization;

namespace Trivia.Api.Models;

public record TriviaApiResponse(
    [property: JsonPropertyName("response_code")] int ResponseCode,
    [property: JsonPropertyName("results")] List<TriviaQuestion> Results
);

public record TriviaQuestion(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("difficulty")] string Difficulty,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("correct_answer")] string CorrectAnswer,
    [property: JsonPropertyName("incorrect_answers")] List<string> IncorrectAnswers
);