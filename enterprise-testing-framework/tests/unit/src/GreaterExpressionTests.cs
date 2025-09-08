// Curated test from internal project; sanitized for portfolio use.
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestLib.Expressions;

namespace TestLib.Tests;

[TestFixture]
public class GreaterExpressionTests
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
    public void Evaluate_IntegerGreater_ReturnsTrue()
    {
        var expression = new GreaterExpression(new ConstantExpression(50), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_IntegerNotGreater_ReturnsFalse()
    {
        var expression = new GreaterExpression(new ConstantExpression(30), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_IntegerEqual_ReturnsFalse()
    {
        var expression = new GreaterExpression(new ConstantExpression(42), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_LongGreater_ReturnsTrue()
    {
        var expression = new GreaterExpression(new ConstantExpression(100L), new ConstantExpression(50L));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringGreater_ReturnsTrue()
    {
        var expression = new GreaterExpression(new ConstantExpression("zebra"), new ConstantExpression("apple"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringNotGreater_ReturnsFalse()
    {
        var expression = new GreaterExpression(new ConstantExpression("apple"), new ConstantExpression("zebra"));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_IntStringComparison_ReturnsTrue()
    {
        var expression = new GreaterExpression(new ConstantExpression(50), new ConstantExpression("42"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_StringIntComparison_ReturnsFalse()
    {
        var expression = new GreaterExpression(new ConstantExpression("30"), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_WithJsonPathExpression_ReturnsTrue()
    {
        var expression = new GreaterExpression(new ConstantExpression(50), new JsonPathExpression("$var:intValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_WithJsonPathExpression_ReturnsFalse()
    {
        var expression = new GreaterExpression(new ConstantExpression(30), new JsonPathExpression("$var:intValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_InvalidComparison_HandlesGracefully()
    {
        var expression = new GreaterExpression(new ConstantExpression("invalid"), new ConstantExpression(new object()));
        var result = expression.Evaluate(_testContext);
        Assert.IsInstanceOf<bool>(result);
    }

    [Test]
    public void GetErrorMessage_ReturnsExpectedFormat()
    {
        var expression = new GreaterExpression(new ConstantExpression(30), new ConstantExpression(42));
        expression.Evaluate(_testContext);
        var errorMessage = expression.GetErrorMessage();
        StringAssert.Contains("Greater-than assertion failed", errorMessage);
        StringAssert.Contains("Expression1 should be greater than Expression2", errorMessage);
    }

    [Test]
    public void GetErrorMessage_WithCustomMessage_ReturnsCustomMessage()
    {
        var customMessage = "Custom greater than error message";
        var expression = new GreaterExpression(new ConstantExpression(30), new ConstantExpression(42), customMessage);
        expression.Evaluate(_testContext);
        var errorMessage = expression.GetErrorMessage();
        Assert.AreEqual(customMessage, errorMessage);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var expression = new GreaterExpression(new ConstantExpression(50), new ConstantExpression(42));
        expression.Evaluate(_testContext);
        var toStringResult = expression.ToString();
        StringAssert.Contains("Greater", toStringResult);
    }
}
