// Curated test from internal project; sanitized for portfolio use.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using TestLib.Assertions;
using TestLib.Cmd;
using NUnitAssert = NUnit.Framework.Assert;

namespace TestLib.Tests;

[TestFixture]
public class PerformanceDataAccessTests
{
    private TestContext _testContext;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
    }

    [Test]
    public void Asserter_ShouldAccessPerformanceData_DuringExecution()
    {
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 0)),
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.assertionTimeMs"), ("ConstExpr", 0)),
            AsserterFactory.Create("AssertNotNull", ("JPathExpr", "$curStep:performance.stepType"))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Hello, Performance Test!" }
        };

        var step = new TestStep
        {
            Name = "Performance Test Step",
            Description = "Test step that validates performance data access",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var result = step.Execute(_testContext);

        NUnitAssert.IsTrue(result.Passed);
        NUnitAssert.AreEqual(0, result.AssertionFailures.Count);

        var performanceData = _testContext.ResolveJPath("$curStep:performance");
        NUnitAssert.IsNotNull(performanceData);
        NUnitAssert.IsNotNull(performanceData["executionTimeMs"]);
        NUnitAssert.IsNotNull(performanceData["assertionTimeMs"]);
        NUnitAssert.IsNotNull(performanceData["startTime"]);
        NUnitAssert.IsNotNull(performanceData["endTime"]);
        NUnitAssert.IsNotNull(performanceData["stepType"]);
        NUnitAssert.GreaterOrEqual((long)performanceData["executionTimeMs"], 0);
        NUnitAssert.GreaterOrEqual((long)performanceData["assertionTimeMs"], 0);
    }

    [Test]
    public void Asserter_ShouldFailGracefully_WhenPerformanceDataNotAvailable()
    {
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("ExpectNotNull", ("JPathExpr", "$curStep:performance.nonExistentField"))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Hello, Test!" }
        };

        var step = new TestStep
        {
            Name = "Performance Test Step",
            Description = "Test step that tries to access non-existent performance data",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var result = step.Execute(_testContext);

        NUnitAssert.IsFalse(result.Passed);
        NUnitAssert.Greater(result.AssertionFailures.Count, 0);
    }

    [Test]
    public void PerformanceTesting_ShouldValidateExecutionTimeThresholds()
    {
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertLt", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 1000)),
            AsserterFactory.Create("AssertLt", ("JPathExpr", "$curStep:performance.assertionTimeMs"), ("ConstExpr", 500))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Performance threshold test" }
        };

        var step = new TestStep
        {
            Name = "Performance Threshold Test",
            Description = "Test step that validates execution time thresholds",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var result = step.Execute(_testContext);
        NUnitAssert.IsTrue(result.Passed);

        var performanceData = _testContext.ResolveJPath("$curStep:performance");
        var executionTime = (long)performanceData["executionTimeMs"];
        var assertionTime = (long)performanceData["assertionTimeMs"];
        NUnitAssert.Less(executionTime, 1000);
        NUnitAssert.Less(assertionTime, 500);
    }

    [Test]
    public void PerformanceTesting_ShouldMeasureSlowOperations()
    {
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 0)),
            AsserterFactory.Create("AssertLt", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 10000)),
            AsserterFactory.Create("AssertEq", ("JPathExpr", "$curStep:performance.stepType"), ("ConstExpr", "cmd"))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "ping",
            Arguments = new List<string> { "127.0.0.1", "-n", "2" }
        };

        var step = new TestStep
        {
            Name = "Slow Operation Test",
            Description = "Test step that measures performance of slower operations",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var result = step.Execute(_testContext);
        NUnitAssert.IsTrue(result.Passed);

        var performanceData = _testContext.ResolveJPath("$curStep:performance");
        var executionTime = (long)performanceData["executionTimeMs"];
        NUnitAssert.GreaterOrEqual(executionTime, 0);
        NUnitAssert.Less(executionTime, 5000);
    }

    [Test]
    public void PerformanceTesting_ShouldValidateTimestamps()
    {
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertNotNull", ("JPathExpr", "$curStep:performance.startTime")),
            AsserterFactory.Create("AssertNotNull", ("JPathExpr", "$curStep:performance.endTime"))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Timestamp validation test" }
        };

        var step = new TestStep
        {
            Name = "Timestamp Validation Test",
            Description = "Test step that validates timing metadata",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var startTestTime = DateTime.UtcNow;
        var result = step.Execute(_testContext);
        var endTestTime = DateTime.UtcNow;
        NUnitAssert.IsTrue(result.Passed);

        var performanceData = _testContext.ResolveJPath("$curStep:performance");
        var startTime = DateTime.Parse(performanceData["startTime"].ToString());
        NUnitAssert.GreaterOrEqual(startTime, startTestTime.AddSeconds(-1));
        NUnitAssert.LessOrEqual(startTime, endTestTime.AddSeconds(1));
    }

    [Test]
    public void PerformanceTesting_ShouldCompareRelativePerformance()
    {
        var fastStepAsserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 0))
        };

        var fastInputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Fast operation" }
        };

        var fastStep = new TestStep
        {
            Name = "Fast Step",
            Description = "Fast operation for performance comparison",
            InputTemplate = fastInputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = fastStepAsserters
        };

        fastStep.Initialize(_testContext);
        var fastResult = fastStep.Execute(_testContext);
        var fastPerformanceData = _testContext.ResolveJPath("$curStep:performance");
        var fastExecutionTime = (long)fastPerformanceData["executionTimeMs"];

        var slowContext = new TestContext();
        var slowStepAsserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 0))
        };

        var slowInputTemplate = new CmdTestStepInputTemplate
        {
            Command = "ping",
            Arguments = new List<string> { "127.0.0.1", "-n", "1" }
        };

        var slowStep = new TestStep
        {
            Name = "Slower Step",
            Description = "Slower operation for performance comparison",
            InputTemplate = slowInputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = slowStepAsserters
        };

        slowStep.Initialize(slowContext);
        var slowResult = slowStep.Execute(slowContext);
        var slowPerformanceData = slowContext.ResolveJPath("$curStep:performance");
        var slowExecutionTime = (long)slowPerformanceData["executionTimeMs"];

        NUnitAssert.IsTrue(fastResult.Passed);
        NUnitAssert.IsTrue(slowResult.Passed);
        System.Console.WriteLine($"Fast operation: {fastExecutionTime}ms, Slower operation: {slowExecutionTime}ms");
        NUnitAssert.GreaterOrEqual(fastExecutionTime, 0);
        NUnitAssert.GreaterOrEqual(slowExecutionTime, 0);
    }

    [Test]
    public void PerformanceTesting_ShouldValidatePerformanceRegression()
    {
        var baselineExecutionTime = 1000; // ms
        var asserters = new List<Asserter>
        {
            AsserterFactory.Create("AssertLt", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", baselineExecutionTime * 2)),
            AsserterFactory.Create("AssertGte", ("JPathExpr", "$curStep:performance.executionTimeMs"), ("ConstExpr", 0))
        };

        var inputTemplate = new CmdTestStepInputTemplate
        {
            Command = "echo",
            Arguments = new List<string> { "Performance regression test" }
        };

        var step = new TestStep
        {
            Name = "Performance Regression Test",
            Description = "Test step that validates performance hasn't regressed",
            InputTemplate = inputTemplate,
            InputFactory = new CmdTestStepInputFactory(),
            Executor = new CmdTestStepExecutor(new CmdTestStepOutputSettings(), new TestStepExecutionSettings()),
            Type = "cmd",
            Asserters = asserters
        };

        step.Initialize(_testContext);
        var result = step.Execute(_testContext);
        NUnitAssert.IsTrue(result.Passed);

        var performanceData = _testContext.ResolveJPath("$curStep:performance");
        var actualExecutionTime = (long)performanceData["executionTimeMs"];
        System.Console.WriteLine($"Baseline: {baselineExecutionTime}ms, Actual: {actualExecutionTime}ms");
        NUnitAssert.Less(actualExecutionTime, baselineExecutionTime * 2);
    }
}
