using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Trivia.Api.Controllers;
using Trivia.Api.Data;
using Trivia.Api.Models;
using Trivia.Api.Tests.Helpers;
using Trivia.Shared.Models;
using Xunit;

namespace Trivia.Api.Tests;

public class TriviaControllerTests
{
    private static TriviaController CreateController(TriviaDbContext db)
    {
        var config = new ConfigurationBuilder().Build();
        return new TriviaController(null!, db, config);
    }

    private static async Task SeedTestData(TriviaDbContext db)
    {
        db.Questions.AddRange(
            new TriviaQuestionEntity
            {
                Question = "What is 2+2?",
                Category = "Math",
                Difficulty = "Easy",
                FetchedAt = DateTimeOffset.UtcNow,
                Answers =
                [
                    new TriviaAnswerEntity { Text = "4", IsCorrect = true },
                    new TriviaAnswerEntity { Text = "3", IsCorrect = false },
                    new TriviaAnswerEntity { Text = "5", IsCorrect = false }
                ]
            },
            new TriviaQuestionEntity
            {
                Question = "What is the capital of France?",
                Category = "Geography",
                Difficulty = "Easy",
                FetchedAt = DateTimeOffset.UtcNow,
                Answers =
                [
                    new TriviaAnswerEntity { Text = "Paris", IsCorrect = true },
                    new TriviaAnswerEntity { Text = "London", IsCorrect = false }
                ]
            },
            new TriviaQuestionEntity
            {
                Question = "What is quantum entanglement?",
                Category = "Science",
                Difficulty = "Hard",
                FetchedAt = DateTimeOffset.UtcNow,
                Answers =
                [
                    new TriviaAnswerEntity { Text = "Particle correlation", IsCorrect = true },
                    new TriviaAnswerEntity { Text = "Gravity", IsCorrect = false }
                ]
            }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetQuestionsCount_ReturnsTotalCount()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetQuestionsCount();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(3, ok.Value);
    }

    [Fact]
    public async Task GetCategories_ReturnsDistinctSortedCategories()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetCategories();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var categories = Assert.IsAssignableFrom<List<string>>(ok.Value);
        Assert.Equal(["Geography", "Math", "Science"], categories);
    }

    [Fact]
    public async Task GetDifficulties_ReturnsInCorrectOrder()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetDifficulties();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var difficulties = Assert.IsAssignableFrom<IEnumerable<string>>(ok.Value);
        Assert.Equal(["Easy", "Hard"], difficulties);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task GetQuizQuestions_ReturnsBadRequestForInvalidAmount(int amount)
    {
        await using var db = TestDbContext.Create();
        var controller = CreateController(db);

        var result = await controller.GetQuizQuestions(amount: amount);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetQuizQuestions_ReturnsRequestedAmount()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetQuizQuestions(amount: 2);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var questions = Assert.IsAssignableFrom<List<QuizQuestion>>(ok.Value);
        Assert.Equal(2, questions.Count);
    }

    [Fact]
    public async Task GetQuizQuestions_FiltersByCategory()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetQuizQuestions(category: "Math", amount: 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var questions = Assert.IsAssignableFrom<List<QuizQuestion>>(ok.Value);
        Assert.Single(questions);
        Assert.Equal("Math", questions[0].Category);
    }

    [Fact]
    public async Task GetQuizQuestions_FiltersByDifficulty()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var result = await controller.GetQuizQuestions(difficulty: "Hard", amount: 10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var questions = Assert.IsAssignableFrom<List<QuizQuestion>>(ok.Value);
        Assert.Single(questions);
        Assert.Equal("Hard", questions[0].Difficulty);
    }

    [Fact]
    public async Task CheckAnswer_ReturnsCorrectForRightAnswer()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var question = db.Questions.First(q => q.Question == "What is 2+2?");
        var correctAnswer = question.Answers.First(a => a.IsCorrect);

        var result = await controller.CheckAnswer(new AnswerSubmission(question.Id, correctAnswer.Id));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var answerResult = Assert.IsType<AnswerResult>(ok.Value);
        Assert.True(answerResult.IsCorrect);
        Assert.Equal("4", answerResult.CorrectAnswerText);
    }

    [Fact]
    public async Task CheckAnswer_ReturnsIncorrectForWrongAnswer()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var question = db.Questions.First(q => q.Question == "What is 2+2?");
        var wrongAnswer = question.Answers.First(a => !a.IsCorrect);

        var result = await controller.CheckAnswer(new AnswerSubmission(question.Id, wrongAnswer.Id));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var answerResult = Assert.IsType<AnswerResult>(ok.Value);
        Assert.False(answerResult.IsCorrect);
        Assert.Equal("4", answerResult.CorrectAnswerText);
    }

    [Fact]
    public async Task CheckAnswer_ReturnsNotFoundForMissingQuestion()
    {
        await using var db = TestDbContext.Create();
        var controller = CreateController(db);

        var result = await controller.CheckAnswer(new AnswerSubmission(999, 1));

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task CheckAnswer_ReturnsNotFoundForMissingAnswer()
    {
        await using var db = TestDbContext.Create();
        await SeedTestData(db);
        var controller = CreateController(db);

        var question = db.Questions.First();

        var result = await controller.CheckAnswer(new AnswerSubmission(question.Id, 999));

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(51)]
    public async Task SeedQuestions_ReturnsBadRequestForInvalidAmount(int amount)
    {
        await using var db = TestDbContext.Create();
        var controller = CreateController(db);

        var result = await controller.SeedQuestions(amount: amount);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
