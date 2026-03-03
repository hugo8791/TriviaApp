namespace Trivia.Shared.Models;

public record QuizAnswer(int Id, string Text);

public record QuizQuestion(int Id, string Question, string Category, string Difficulty, List<QuizAnswer> Answers);

public record AnswerSubmission(int QuestionId, int AnswerId);

public record AnswerResult(bool IsCorrect, string CorrectAnswerText);