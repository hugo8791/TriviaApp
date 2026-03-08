# TriviaApp

A full-stack trivia application built with **ASP.NET Core** (backend) and **Blazor WebAssembly** (frontend). Questions are sourced from the Open Trivia Database and stored locally in PostgreSQL so that the correct answers are never exposed to the client.

## Architecture

- **Trivia.Api**: ASP.NET Core Web API
- **Trivia.Client**: Blazor WebAssembly frontend
- **Trivia.Shared**: shared models
- **Trivia.Api.Tests**: unit tests

The API acts as a proxy between the client and OpenTDB. Answers are stored server-side, so a user inspecting network traffic only ever sees question text and shuffled answer options, never which answer is correct.

## Prerequisites

- .NET 10 SDK
- PostgreSQL (or Docker to run one)
- Docker is only needed to run PostgreSQL locally (not required for tests)

## Running locally

### 1. Start a PostgreSQL database

```bash
docker compose up -d
```

Or point to any existing PostgreSQL instance.

### 2. Configure the API

The connection string in `src/Trivia.Api/appsettings.Development.json` is already set up to match the Docker Compose defaults, so no changes are needed if you used step 1.

### 3. Apply database migrations

```bash
dotnet ef database update --project src/Trivia.Api
```

### 4. Run the API

```bash
dotnet run --project src/Trivia.Api
```

The port is shown in the console output. An interactive API reference (Scalar) is available at `/scalar/v1`.

### 5. Run the frontend

```bash
dotnet run --project src/Trivia.Client
```

The port is shown in the console output. API calls are proxied to the backend automatically in development.

### 6. Seed questions

Before playing, you need questions in the database. Navigate to the **Add Questions** page in the UI, choose how many to fetch (1–50), and click **Add Questions**. This calls `POST /trivia/seed`, which fetches questions from OpenTDB and stores them, skipping any duplicates already in the database.

## API reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/trivia/count` | Total number of stored questions |
| `GET` | `/trivia/categories` | Distinct categories (alphabetical) |
| `GET` | `/trivia/difficulties` | Distinct difficulties (Easy → Medium → Hard) |
| `GET` | `/trivia/questions` | Fetch quiz questions (no correct answer exposed). Query params: `amount` (1–20, default 10), `category`, `difficulty` |
| `POST` | `/trivia/checkanswer` | Submit an answer. Body: `{ "questionId": int, "answerId": int }` |
| `POST` | `/trivia/seed` | Fetch questions from OpenTDB and store them. Query param: `amount` (1–50, default 10) |

## Running the tests

Tests use xUnit v3 with an **in-memory EF Core database**, so no PostgreSQL or Docker is needed to run them.

```bash
dotnet test
```

## Deployment

The application is deployed automatically to Render on every push to `main` via GitHub Actions. The workflow builds the solution, runs all tests, and triggers a Render deploy hook if everything passes.