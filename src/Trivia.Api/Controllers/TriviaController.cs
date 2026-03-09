using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trivia.Api.Data;
using Trivia.Api.Models;
using Trivia.Api.Transformers;
using Trivia.Shared.Models;

namespace Trivia.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TriviaController(IHttpClientFactory httpClientFactory, TriviaDbContext dbContext, IConfiguration configuration) : ControllerBase
{
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetQuestionsCount()
    {
        return Ok(await dbContext.Questions.CountAsync());
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories()
    {
        var categories = await dbContext.Questions
            .Select(q => q.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("difficulties")]
    public async Task<ActionResult<List<string>>> GetDifficulties()
    {
        var difficulties = await dbContext.Questions
            .Select(q => q.Difficulty)
            .Distinct()
            .ToListAsync();

        return Ok(difficulties.OrderBy(d => d.ToLower() switch
        {
            "easy" => 0,
            "medium" => 1,
            "hard" => 2,
            _ => 3
        }));
    }

    [HttpGet("questions")]
    public async Task<ActionResult<List<QuizQuestion>>> GetQuizQuestions(
        [FromQuery] string? category = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] int amount = 10)
    {
        if (amount is < 1 or > 20)
            return BadRequest("Amount must be between 1 and 20.");

        var query = dbContext.Questions
            .Include(q => q.Answers)
            .AsQueryable();

        if (category is not null)
            query = query.Where(q => q.Category == category);

        if (difficulty is not null)
            query = query.Where(q => q.Difficulty == difficulty);

        var all = await query.ToListAsync();

        var selected = all
            .OrderBy(_ => Random.Shared.Next())
            .Take(amount)
            .Select(TriviaEntityTransformer.ToQuizQuestion)
            .ToList();

        return Ok(selected);
    }

    [HttpPost("checkanswer")]
    public async Task<ActionResult<AnswerResult>> CheckAnswer([FromBody] AnswerSubmission submission)
    {
        var question = await dbContext.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == submission.QuestionId);

        if (question is null)
            return NotFound("Question not found.");

        var selectedAnswer = question.Answers.FirstOrDefault(a => a.Id == submission.AnswerId);
        if (selectedAnswer is null)
            return NotFound("Answer not found.");

        var correctAnswer = question.Answers.First(a => a.IsCorrect);

        return Ok(new AnswerResult(selectedAnswer.IsCorrect, correctAnswer.Text));
    }

    [HttpPost("seed")]
    public async Task<ActionResult<SeedResult>> SeedQuestions([FromQuery] int amount = 10)
    {
        if (amount is < 1 or > 50)
            return BadRequest("Amount must be between 1 and 50.");

        var client = httpClientFactory.CreateClient();
        var baseUrl = configuration["OpenTdb:BaseUrl"]
            ?? throw new InvalidOperationException("OpenTdb:BaseUrl is not configured.");

        var httpResponse = await client.GetAsync($"{baseUrl}?amount={amount}");

        if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
            return StatusCode(429, "OpenTDB rate limit reached. Please wait a moment and try again.");

        if (!httpResponse.IsSuccessStatusCode)
            return StatusCode(502, "Failed to retrieve trivia questions from OpenTDB.");

        var response = await httpResponse.Content.ReadFromJsonAsync<TriviaApiResponse>();

        if (response is null || response.ResponseCode != 0)
            return StatusCode(502, "Failed to retrieve trivia questions.");

        var candidates = response.Results
            .Select(OpenTdbTransformer.ToEntity)
            .ToList();

        var incomingTexts = candidates.Select(q => q.Question).ToHashSet();

        var existingTexts = (await dbContext.Questions
            .Where(q => incomingTexts.Contains(q.Question))
            .Select(q => q.Question)
            .ToListAsync())
            .ToHashSet();

        var newEntities = candidates
            .Where(q => !existingTexts.Contains(q.Question))
            .ToList();

        await dbContext.Questions.AddRangeAsync(newEntities);
        await dbContext.SaveChangesAsync();

        return Ok(new SeedResult(newEntities.Count, existingTexts.Count));
    }
}