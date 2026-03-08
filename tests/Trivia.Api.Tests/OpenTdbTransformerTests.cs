using Trivia.Api.Models;
using Trivia.Api.Transformers;
using Xunit;

namespace Trivia.Api.Tests;

public class OpenTdbTransformerTests
{
    [Fact]
    public void ToEntity_DecodesHtmlEntities()
    {
        var resource = new TriviaQuestion(
            Type: "multiple",
            Difficulty: "easy",
            Category: "Science &amp; Nature",
            Question: "What&#039;s the chemical symbol for water?",
            CorrectAnswer: "H&amp;O",
            IncorrectAnswers: ["Na", "K&amp;Cl", "Fe"]
        );

        var entity = OpenTdbTransformer.ToEntity(resource);

        Assert.Equal("Science & Nature", entity.Category);
        Assert.Equal("What's the chemical symbol for water?", entity.Question);
        Assert.Equal("H&O", entity.Answers.First(a => a.IsCorrect).Text);
        Assert.Equal("K&Cl", entity.Answers.First(a => a.Text == "K&Cl").Text);
    }

    [Theory]
    [InlineData("easy", "Easy")]
    [InlineData("medium", "Medium")]
    [InlineData("hard", "Hard")]
    public void ToEntity_CapitalizesDifficulty(string input, string expected)
    {
        var resource = new TriviaQuestion(
            Type: "multiple",
            Difficulty: input,
            Category: "General",
            Question: "Test?",
            CorrectAnswer: "Yes",
            IncorrectAnswers: ["No"]
        );

        var entity = OpenTdbTransformer.ToEntity(resource);

        Assert.Equal(expected, entity.Difficulty);
    }

    [Fact]
    public void ToEntity_MapsCorrectAndIncorrectAnswers()
    {
        var resource = new TriviaQuestion(
            Type: "multiple",
            Difficulty: "easy",
            Category: "General",
            Question: "Test?",
            CorrectAnswer: "Right",
            IncorrectAnswers: ["Wrong1", "Wrong2", "Wrong3"]
        );

        var entity = OpenTdbTransformer.ToEntity(resource);

        Assert.Equal(4, entity.Answers.Count);
        Assert.Single(entity.Answers, a => a.IsCorrect);
        Assert.Equal(3, entity.Answers.Count(a => !a.IsCorrect));
        Assert.Equal("Right", entity.Answers.Single(a => a.IsCorrect).Text);
    }
}
