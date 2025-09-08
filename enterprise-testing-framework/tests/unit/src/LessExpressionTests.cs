// Curated test from internal project; sanitized for portfolio use.
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestLib.Expressions;

namespace TestLib.Tests;

[TestFixture]
public class LessExpressionTests
{
    private TestContext _testContext;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
        _testContext.Variables.Upsert("intValue", "42");
        _testContext.Variables.Upsert("floatValue", "3.14");
        _testContext.Variables.Upsert("stringValue", "hello");
        _testContext.Variables.Upsert("jsonValue", "{\"intValue\":10, \"stringValue\":\"world\"}");
    }

    [Test]
    public void Evaluate_IntegerLess_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(30), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_IntegerNotLess_ReturnsFalse()
    {
        var expression = new LessExpression(new ConstantExpression(50), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_IntegerEqual_ReturnsFalse()
    {
        var expression = new LessExpression(new ConstantExpression(42), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_LongLess_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(50L), new ConstantExpression(100L));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_IntLongComparison_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(30), new ConstantExpression(100L));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_LongIntComparison_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(30L), new ConstantExpression(100));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringLess_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression("apple"), new ConstantExpression("zebra"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringNotLess_ReturnsFalse()
    {
        var expression = new LessExpression(new ConstantExpression("zebra"), new ConstantExpression("apple"));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_IntStringComparison_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(30), new ConstantExpression("42"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringIntComparison_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression("30"), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringJTokenComparison_ReturnsTrue()
    {
        var token = JToken.FromObject("zebra");
        var expression = new LessExpression(new ConstantExpression("apple"), new ConstantExpression(token));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_JTokenStringComparison_ReturnsTrue()
    {
        var token = JToken.FromObject("apple");
        var expression = new LessExpression(new ConstantExpression(token), new ConstantExpression("zebra"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_WithJsonPathExpression_ReturnsTrue()
    {
        var expression = new LessExpression(new ConstantExpression(30), new JsonPathExpression("$var:intValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_WithJsonPathExpression_ReturnsFalse()
    {
        var expression = new LessExpression(new ConstantExpression(50), new JsonPathExpression("$var:intValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_InvalidComparison_HandlesGracefully()
    {
        var expression = new LessExpression(new ConstantExpression(new object()), new ConstantExpression("invalid"));
        var result = expression.Evaluate(_testContext);
        Assert.IsInstanceOf<bool>(result);
    }

    [Test]
    public void GetErrorMessage_ReturnsExpectedFormat()
    {
        var expression = new LessExpression(new ConstantExpression(50), new ConstantExpression(42));
        expression.Evaluate(_testContext);
        var errorMessage = expression.GetErrorMessage();
        StringAssert.Contains("Less-than assertion failed", errorMessage);
        StringAssert.Contains("Expression1 should be less than Expression2", errorMessage);
    }

    [Test]
    public void GetErrorMessage_WithCustomMessage_ReturnsCustomMessage()
    {
        var customMessage = "Custom less than error message";
        var expression = new LessExpression(new ConstantExpression(50), new ConstantExpression(42), customMessage);
        expression.Evaluate(_testContext);
        var errorMessage = expression.GetErrorMessage();
        Assert.AreEqual(customMessage, errorMessage);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var expression = new LessExpression(new ConstantExpression(30), new ConstantExpression(42));
        expression.Evaluate(_testContext);
        var toStringResult = expression.ToString();
        StringAssert.Contains("Less", toStringResult);
    }
}
