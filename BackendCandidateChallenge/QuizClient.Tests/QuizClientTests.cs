using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using PactNet;
using PactNet.Output.Xunit;
using Xunit;
using Xunit.Abstractions;

namespace QuizClient.Tests;

public class QuizClientTests
{
    private readonly IPactBuilderV4 _pact;
    private readonly Mock<IHttpClientFactory> _mockFactory;

    public QuizClientTests(ITestOutputHelper testOutput)
    {
        var config = new PactConfig
        {
            PactDir = @"..\pacts",
            Outputters =
            [
                new XunitOutput(testOutput)
            ],
            DefaultJsonSettings = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            }
        };

        _pact = Pact.V4("QuizClient", "QuizService", config).WithHttpInteractions();

        _mockFactory = new Mock<IHttpClientFactory>();
    }

    [Fact]
    public async Task GetQuizzes_WhenSomeQuizzesExists_ReturnsTheQuizzes()
    {
        _pact
            .UponReceiving("A GET request to retrieve the quizzes")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Get, "/api/quizzes")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new[]
                {
                    new
                    {
                        id = 123,
                        title = "This is quiz 123"
                    },
                    new
                    {
                        id = 124,
                        title = "This is quiz 124"
                    }
                });

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri, _mockFactory.Object.CreateClient("Quiz"));
            var result = await client.GetQuizzesAsync(CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEmpty(result.Value);
            Assert.Equal(2, result.Value.Count());
        });
    }

    [Fact]
    public async Task GetQuiz_WhenAQuizWExists_ReturnsTheQuiz()
    {
        _pact
            .UponReceiving("A GET request to retrieve the quiz")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Get, "/api/quizzes/123")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = 123,
                    title = "This is quiz 123"
                });

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.GetQuizAsync(123, CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotEqual(Quiz.NotFound, result.Value);
            Assert.Equal("This is quiz 123", result.Value.Title);
        });
    }

    [Fact]
    public async Task PostQuiz_Returns201CreatedAndLocationHeader()
    {
        _pact
            .UponReceiving("A POST quiz request")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Post, "/api/quizzes")
                .WithHeader("Content-Type", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Location", PactNet.Matchers.Match.Regex("/api/quizzes/1", "quizzes\\/[0-9]*"));

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.PostQuizAsync(new Quiz { Title = "This is quiz 999" }, CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Value);
        });
    }

    [Fact]
    public async Task PostQuestion_Returns201CreatedAndLocationHeader()
    {
        _pact
            .UponReceiving("A POST request to quiz 123 questions collection")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Post, "/api/quizzes/123/questions")
                .WithHeader("Content-Type", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Location", PactNet.Matchers.Match.Regex("/api/quizzes/123/questions/1", "quizzes\\/123\\/questions\\/[0-9]*"));

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.PostQuestionAsync(123, new QuizQuestion { Text = "This is a question" }, CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Value);
        });
    }

    [Fact]
    public async Task PutQuestion_WhenAQuestionWExists_UpdatesTheQuestion()
    {
        _pact
            .UponReceiving("A PUT request to update a quiz question with id = 1")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Put, "/api/quizzes/123/questions/1")
                .WithHeader("Content-Type", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.NoContent)
                .WithHeader("Content-Type", "application/json; charset=utf-8");

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.PutQuestionAsync(123, 1, new QuizQuestion { Text = "Updated text" },
                CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            Assert.NotEqual(Quiz.NotFound, result.Value);
        });
    }
		
    [Fact]
    public async Task PostAnswers_Returns201CreatedAndLocationHeader()
    {
        _pact
            .UponReceiving("A POST request")
                .Given("There are some quizzes")
                .WithRequest(HttpMethod.Post, "/api/quizzes")
                .WithHeader("Content-Type", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Location", PactNet.Matchers.Match.Regex("/api/quizzes/1", "quizzes\\/[0-9]*"))
                .WithJsonBody(new
                {
                    title = "This is quiz 999"
                });

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.PostQuizAsync(new Quiz { Title = "This is quiz 999" }, CancellationToken.None);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Value);
        });
    }

    [Fact]
    public async Task GivenThatAQuizExistsPostingAnAnswerCreatesAQuizResponse()
    {
        _pact
            .UponReceiving("A POST request creates a quiz response")
                .Given("There is a quiz with id '123'")
                .WithRequest(HttpMethod.Post, "/api/quizzes/123/responses")
                .WithHeader("Content-Type", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.Created)
                .WithHeader("Content-Type", "application/json")
                .WithHeader("Location", PactNet.Matchers.Match.Regex("/api/quizzes/123/responses/1", "responses\\/[0-9]*"));

        await _pact.VerifyAsync(async ctx =>
        {
            _mockFactory
                .Setup(f => f.CreateClient("Quiz"))
                .Returns(() => new HttpClient
                {
                    BaseAddress = ctx.MockServerUri,
                    DefaultRequestHeaders = { Accept = { MediaTypeWithQualityHeaderValue.Parse("application/json") } }
                });

            var client = new QuizClient(ctx.MockServerUri,_mockFactory.Object.CreateClient("Quiz"));
            var result = await client.PostQuizResponseAsync(new QuestionResponse(), 123);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage), result.ErrorMessage);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
            Assert.NotNull(result.Value);
        });
    }
}