// Curated test from internal project; sanitized for portfolio use.
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace TestLib.Tests;

[TestFixture]
public class TestContextResolveTests
{
    private TestContext _testContext;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
        _testContext.UpsertName("Comprehensive Test");
        _testContext.Variables.Upsert("access_token", "test_access_token");
        _testContext.AddStepInput("step1_id", new JObject { ["key1"] = "value1", ["key2"] = "value2" });
        _testContext.AddStepOutput("step1_id", new JObject { ["statusCode"] = 200, ["content"] = new JObject { ["message"] = "OK", ["data"] = "sample data" } });
        _testContext.SetValueToStep("step1_id", "name", JToken.FromObject("step1"));
        _testContext.AddStepInput("step2_id", new JObject { ["key1"] = "value3", ["key2"] = "value4" });
        _testContext.AddStepOutput("step2_id", new JObject { ["statusCode"] = 404, ["content"] = new JObject { ["error"] = "Not Found" } });
        _testContext.SetValueToStep("step2_id", "name", JToken.FromObject("step2"));
        _testContext.AddStepInput("last_step_id", new JObject { ["key1"] = "last_value1", ["key2"] = "last_value2" });
        _testContext.AddStepOutput("last_step_id", new JObject { ["statusCode"] = 202, ["content"] = new JObject { ["accessToken"] = "abcdef123456", ["expiry"] = "2025-12-31T23:59:59Z" } });
        _testContext.SetValueToStep("last_step_id", "name", JToken.FromObject("last_step"));
        _testContext.AddStepInput("current_step_id", new JObject { ["key1"] = "current_value1", ["key2"] = "current_value2" });
        _testContext.SetValueToStep("current_step_id", "name", JToken.FromObject("current_step"));
        _testContext.AddStepInput("step4_id", new JObject { ["key1"] = "value5", ["key2"] = "value6" });
        _testContext.SetValueToStep("step4_id", "name", JToken.FromObject("step4"));
    }

    [TestCase("$.test.name", "Comprehensive Test", TestName = "GetTestName")]
    [TestCase("$.test.steps[0].name", "step1", TestName = "GetFirstStepName")]
    [TestCase("$.test.steps[1].input.key1", "value3", TestName = "GetSecondStepInputKey1")]
    [TestCase("$curStep:$.input.key2", "current_value2", TestName = "GetCurrentStepInputKey2")]
    [TestCase("$.test.steps[2].output.statusCode", "202", TestName = "GetSecondStepStatusCode")]
    [TestCase("$.test.steps[?(@.name=='current_step')].input.key1", "current_value1", TestName = "GetStepkeyWithStepName")]
    [TestCase("$.test.steps[4].input.key2", "value6", TestName = "GetLastStepInputKey2WithIndex")]
    [TestCase("$curStep:$.input.key1", "current_value1", TestName = "GetCurrentStepInputKey1")]
    [TestCase("$curStep:$.name", "current_step", TestName = "GetCurrentStepName")]
    [TestCase("$.test.steps[0].output.statusCode", "200", TestName = "GetFirstStepResponseStatusCode")]
    [TestCase("$.test.steps[0].output.content.message", "OK", TestName = "GetFirstStepResponseContentMessage")]
    [TestCase("$.test.steps[1].output.statusCode", "404", TestName = "GetSecondStepResponseStatusCode")]
    [TestCase("$.test.steps[1].output.content.error", "Not Found", TestName = "GetSecondStepResponseContentError")]
    [TestCase("$lastStep:$.output.statusCode", "202", TestName = "GetLastStepResponseStatusCode")]
    [TestCase("$lastStep:$.output.content.accessToken", "abcdef123456", TestName = "GetLastStepResponseAccessToken")]
    [TestCase("$lastStep:output.content.accessToken", "abcdef123456", TestName = "GetCurrentStepResponseAccessToken")]
    [TestCase("$.test.steps[-1:].input.key2", "value6", TestName = "GetLastStepInputKey2")]
    [TestCase("$.test.steps[0].input.nonexistent", null, TestName = "GetNonExistentProperty")]
    [TestCase("$.test.steps[10].name", null, TestName = "GetNonExistentStep")]
    [TestCase("$var:access_token", "test_access_token", TestName = "GetResolveVarable")]
    [TestCase("$lastStep:$.input.key1", "last_value1", TestName = "GetLastStepInputKey1")]
    [TestCase("$lastStep:$.name", "last_step", TestName = "GetLastStepName")]
    [TestCase("input.key1", "current_value1", TestName = "GetInputKeyFormat")]
    public void TestResolve(string path, string expected)
    {
        var result = _testContext.ResolveJPath(path);
        Assert.AreEqual(expected, result?.ToString());
    }
}
