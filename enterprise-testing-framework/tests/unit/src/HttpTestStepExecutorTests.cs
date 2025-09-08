// Curated test from internal project; sanitized for portfolio use.
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using TestLib.Http;

namespace TestLib.Tests;

[TestFixture]
public class HttpTestStepExecutorTests
{
    private TestStep CreateTestStep(string url, TestContext testContext, List<Assertions.Asserter> asserters = null)
    {
        var inputTemplate = new HttpTestStepInputTemplate
        {
            Method = HttpMethod.Get,
            RequestUri = url,
            Headers = new Dictionary<string, object>
            {
                { "accept", "application/json" },
                { "authorization", "Bearer token" }
            }
        };

        return new TestStep
        {
            Name = "Step 1",
            Description = "HTTP step",
            InputTemplate = inputTemplate,
            Executor = new HttpTestStepExecutor(new HttpTestStepOutputSettings { OutputFormat = "text" }, new TestStepExecutionSettings())
            {
                BaseAddress = new Uri("http://example.com")
            },
            InputFactory = new HttpTestStepInputFactory(),
            Type = "http",
            Asserters = asserters
        };
    }

    private void ExecuteAndAssertStep(TestStep step, TestContext testContext, string expectedStatusCode)
    {
        step.Execute(testContext);
        Assert.NotNull(step.Output, "The step output should not be null.");
        Assert.IsInstanceOf<HttpTestStepOutput>(step.Output);
    }

    [Test]
    public void Execution_Failed_Asserter_ShouldFail_OnIncorrectStatusCode()
    {
        var testContext = new TestContext();
        var asserters = new List<Assertions.Asserter>
        {
            Assertions.AsserterFactory.Create("AssertEq", ("ConstExpr", 200), ("JPathExpr", "output.statusCode"))
        };
        var step = CreateTestStep("http://example.com/api", testContext, asserters);
        step.Initialize(testContext);

        var result = step.Execute(testContext);
        Assert.IsTrue(result.AssertionFailures.Contains(
            "Equality assertion failed: Expression1 should be equal to Expression2.\nExpression1: Constant('200') resolved to 200 \nExpression2: JsonPath('output.statusCode') resolved to 404"));
    }

    [Test]
    public void Execution_Failed_Asserter_ShouldFail_AssertionInvalidException()
    {
        var testContext = new TestContext();
        var asserters = new List<Assertions.Asserter>
        {
            Assertions.AsserterFactory.Create("AssertEq", ("ConstExpr", 200), ("JPathExpr", "output.Status"))
        };
        var step = CreateTestStep("http://example.com/api", testContext, asserters);
        step.Initialize(testContext);

        var result = step.Execute(testContext);
        Assert.IsTrue(result.AssertionFailures.Contains(
            "Equality assertion failed: Expression1 should be equal to Expression2.\nExpression1: Constant('200') resolved to 200 \nExpression2: JsonPath('output.Status') resolved to null"));
    }
}
