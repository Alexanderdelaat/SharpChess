using System.Globalization;
using System.Net.Http.Json;
using NBomber.Contracts;
using NBomber.Contracts.Stats;
using NBomber.CSharp;

const string DefaultBaseUrl = "http://localhost:8080";
const int ReadyP95ThresholdMs = 1_500;
const int AuthP95ThresholdMs = 2_500;

string baseUrl = Environment.GetEnvironmentVariable("PERF_BASE_URL") ?? DefaultBaseUrl;
using HttpClient httpClient = new()
{
    BaseAddress = new Uri(baseUrl),
    Timeout = TimeSpan.FromSeconds(10),
};

Console.WriteLine($"Running SharpChess smoke performance tests against {baseUrl}");

ScenarioProps readinessScenario = Scenario.Create("api_readiness_smoke", async context =>
    await Step.Run("GET /health/ready", context, async () =>
        await SendAsync(() => httpClient.GetAsync("/health/ready"))))
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.Inject(
            rate: 2,
            interval: TimeSpan.FromSeconds(1),
            during: TimeSpan.FromSeconds(30)))
    .WithThresholds(
        Threshold.Create(stats => stats.Fail.Request.Count == 0),
        Threshold.Create("GET /health/ready", stats => stats.Ok.Latency.Percent95 < ReadyP95ThresholdMs));

ScenarioProps authScenario = Scenario.Create("auth_smoke", async context =>
{
    string uniqueSuffix = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}";
    string username = $"perf-{uniqueSuffix}";
    string email = $"perf-{uniqueSuffix}@example.test";
    const string password = "StrongPassword1!";

    IResponse registerResponse = await Step.Run("POST /api/auth/register", context, async () =>
        await SendAsync(() => httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            Username = username,
            Email = email,
            Password = password,
            ConfirmPassword = password,
        })));

    if (!registerResponse.IsError)
    {
        return await Step.Run("POST /api/auth/login", context, async () =>
            await SendAsync(() => httpClient.PostAsJsonAsync("/api/auth/login", new
            {
                Username = username,
                Password = password,
            })));
    }

    return registerResponse;
})
    .WithoutWarmUp()
    .WithLoadSimulations(
        Simulation.Inject(
            rate: 1,
            interval: TimeSpan.FromSeconds(15),
            during: TimeSpan.FromSeconds(60)))
    .WithThresholds(
        Threshold.Create(stats => stats.Fail.Request.Count == 0),
        Threshold.Create("POST /api/auth/register", stats => stats.Ok.Latency.Percent95 < AuthP95ThresholdMs),
        Threshold.Create("POST /api/auth/login", stats => stats.Ok.Latency.Percent95 < AuthP95ThresholdMs));

NodeStats nodeStats = NBomberRunner
    .RegisterScenarios(readinessScenario, authScenario)
    .WithTestSuite("SharpChess")
    .WithTestName("CI smoke performance")
    .WithoutReports()
    .Run(args);

return HasPerformanceRegression(nodeStats) ? 1 : 0;

static async Task<Response<string>> SendAsync(Func<Task<HttpResponseMessage>> send)
{
    try
    {
        using HttpResponseMessage response = await send();
        string responseBody = await response.Content.ReadAsStringAsync();
        string statusCode = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);
        long sizeBytes = responseBody.Length;

        return response.IsSuccessStatusCode
            ? Response.Ok(payload: responseBody, statusCode: statusCode, sizeBytes: sizeBytes)
            : Response.Fail(payload: responseBody, statusCode: statusCode, message: responseBody, sizeBytes: sizeBytes);
    }
    catch (Exception exception)
    {
        return Response.Fail<string>(statusCode: "exception", message: exception.Message);
    }
}

static bool HasPerformanceRegression(NodeStats nodeStats)
{
    bool failed = false;

    foreach (ScenarioStats scenario in nodeStats.ScenarioStats)
    {
        if (scenario.AllFailCount > 0)
        {
            Console.Error.WriteLine(
                $"Scenario '{scenario.ScenarioName}' had {scenario.AllFailCount} failed request(s).");
            failed = true;
        }
    }

    failed |= FailsP95Threshold(nodeStats, "api_readiness_smoke", "GET /health/ready", ReadyP95ThresholdMs);
    failed |= FailsP95Threshold(nodeStats, "auth_smoke", "POST /api/auth/register", AuthP95ThresholdMs);
    failed |= FailsP95Threshold(nodeStats, "auth_smoke", "POST /api/auth/login", AuthP95ThresholdMs);

    return failed;
}

static bool FailsP95Threshold(NodeStats nodeStats, string scenarioName, string stepName, int thresholdMs)
{
    ScenarioStats? scenario = nodeStats.ScenarioStats.FirstOrDefault(stats => stats.ScenarioName == scenarioName);
    StepStats? step = scenario?.StepStats.FirstOrDefault(stats => stats.StepName == stepName);

    if (step is null)
    {
        Console.Error.WriteLine($"Step '{scenarioName}/{stepName}' was not measured.");
        return true;
    }

    if (step.Ok.Request.Count == 0)
    {
        Console.Error.WriteLine($"Step '{scenarioName}/{stepName}' had no successful requests.");
        return true;
    }

    double p95Ms = step.Ok.Latency.Percent95;
    if (p95Ms >= thresholdMs)
    {
        Console.Error.WriteLine(
            $"Step '{scenarioName}/{stepName}' p95 latency was {p95Ms} ms; threshold is {thresholdMs} ms.");
        return true;
    }

    return false;
}
