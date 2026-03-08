using Microsoft.EntityFrameworkCore;
using Trivia.Api.Data;

namespace Trivia.Api.Tests.Helpers;

public static class TestDbContext
{
    public static TriviaDbContext Create()
    {
        var options = new DbContextOptionsBuilder<TriviaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TriviaDbContext(options);
    }
}
