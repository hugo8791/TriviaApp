using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trivia.Api.Data;
using Trivia.Api.Models;
using Trivia.Api.Transformers;

namespace Trivia.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TriviaController(IHttpClientFactory httpClientFactory, TriviaDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetQuestions([FromQuery] string? category = null)
    {
        var query = dbContext.Questions
            .Include(q => q.Answers)
            .AsQueryable();

        if (category is not null)
            query = query.Where(q => q.Category == category);

        return Ok(await query.ToListAsync());
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await dbContext.Questions
            .Select(q => q.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedQuestions([FromQuery] int amount = 10)
    {
        if (amount is < 1 or > 50)
            return BadRequest("Amount must be between 1 and 50.");

        var client = httpClientFactory.CreateClient();
        var response = await client.GetFromJsonAsync<TriviaApiResponse>(
            $"https://opentdb.com/api.php?amount={amount}");

        if (response is null || response.ResponseCode != 0)
            return StatusCode(502, "Failed to retrieve trivia questions.");

        var incomingTexts = response.Results.Select(q => q.Question).ToHashSet();

        var existingTexts = await dbContext.Questions
            .Where(q => incomingTexts.Contains(q.Question))
            .Select(q => q.Question)
            .ToListAsync();

        var newEntities = response.Results
            .Where(q => !existingTexts.Contains(q.Question))
            .Select(TriviaTransformer.ToEntity)
            .ToList();

        await dbContext.Questions.AddRangeAsync(newEntities);
        await dbContext.SaveChangesAsync();

        return Ok(new { saved = newEntities.Count, skipped = existingTexts.Count });
    }
}