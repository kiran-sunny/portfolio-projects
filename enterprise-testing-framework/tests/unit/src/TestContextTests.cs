// Curated test from internal project; sanitized for portfolio use.
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnitAssert = NUnit.Framework.Assert;

namespace TestLib.Tests;

[TestFixture]
public class TestContextTests
{
    private TestContext _testContext;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
    }

    [Test]
    public void AddStepInput_AddsNewStep()
    {
        var input = JToken.FromObject(new { key = "value1" });
        _testContext.AddStepInput("step1", input);
        NUnitAssert.AreEqual("step1", _testContext.ResolveJPath("$.test.steps[0].id").ToString());
        NUnitAssert.AreEqual("value1", _testContext.ResolveJPath("$curStep:$.input.key").ToString());
    }

    [Test]
    public void AddStepInput_ReplacesExistingStepWithInput()
    {
        var input1 = JToken.FromObject(new { key = "value1" });
        var input2 = JToken.FromObject(new { key = "value2" });
        _testContext.AddStepInput("step1", input1);
        _testContext.AddStepInput("step1", input2);
        NUnitAssert.AreEqual("step1", _testContext.ResolveJPath("$.test.steps[0].id").ToString());
        NUnitAssert.AreEqual("value2", _testContext.ResolveJPath("$curStep:$.input.key").ToString());
    }

    [Test]
    public void AddStepOutput_AddsNew()
    {
        var input = JToken.FromObject(new { key = "value1" });
        var output = JToken.FromObject(new { statusCode = 200, key = "value1" });
        _testContext.AddStepInput("step1", input);
        _testContext.AddStepOutput("step1", output);
        NUnitAssert.AreEqual("step1", _testContext.ResolveJPath("$.test.steps[0].id").ToString());
        NUnitAssert.AreEqual("value1", _testContext.ResolveJPath("$curStep:$.input.key").ToString());
        NUnitAssert.AreEqual("200", _testContext.ResolveJPath("$curStep:$.output.statusCode").ToString());
    }
}
