// Curated test from internal project; sanitized for portfolio use.
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using TestLib.Expressions;

namespace TestLib.Tests;

[TestFixture]
public class EqualExpressionTests
{
    private TestContext _testContext;

    [SetUp]
    public void Setup()
    {
        _testContext = new TestContext();
        _testContext.Variables.Upsert("intValue", "42");
        _testContext.Variables.Upsert("floatValue", "3.14");
        _testContext.Variables.Upsert("boolValue", "true");
        _testContext.Variables.Upsert("jsonValue", "{\"intValue\":1, \"boolValue\" : true}");
        _testContext.Variables.Upsert("invalidJson", "not-a-json");
    }

    [Test]
    public void Evaluate_NullValues_ReturnsTrue()
    {
        var expression = new EqualExpression(new ConstantExpression(null), new ConstantExpression(null));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_DifferentNullAndNonNull_ReturnsFalse()
    {
        var expression = new EqualExpression(new ConstantExpression(null), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_ConvertedIntEquality_ReturnsTrue()
    {
        var expression = new EqualExpression(new ConstantExpression("42"), new ConstantExpression(42));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_ConvertedFloatEquality_ReturnsTrue()
    {
        var expression = new EqualExpression(new ConstantExpression("3.14"), new ConstantExpression(3.14f));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_ConvertedBoolEquality_ReturnsTrue()
    {
        var expression = new EqualExpression(new ConstantExpression("true"), new ConstantExpression(true));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_RawStringEquality_ReturnsTrue()
    {
        var expression = new EqualExpression(new ConstantExpression("Hello"), new ConstantExpression("Hello"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_RawStringInequality_ReturnsFalse()
    {
        var expression = new EqualExpression(new ConstantExpression("Hello"), new ConstantExpression("World"));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_JTokenEquality_ReturnsTrue()
    {
        string json1 = "{ \"key\": \"value\", \"array\": [1, 2, 3] }";
        string json2 = "{ \"array\": [1, 2, 3], \"key\": \"value\" }";
        var token1 = JToken.Parse(json1);
        var token2 = JToken.Parse(json2);
        var expression = new EqualExpression(new ConstantExpression(token1), new ConstantExpression(token2));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_JTokenInequality_ReturnsFalse()
    {
        string json1 = "{ \"key\": \"value1\", \"array\": [1, 2, 3] }";
        string json2 = "{ \"key\": \"value2\", \"array\": [1, 2, 3] }";
        var token1 = JToken.Parse(json1);
        var token2 = JToken.Parse(json2);
        var expression = new EqualExpression(new ConstantExpression(token1), new ConstantExpression(token2));
        var result = expression.Evaluate(_testContext);
        Assert.IsFalse((bool)result);
    }

    [Test]
    public void Evaluate_JTokenStringEquality_ReturnsTrue()
    {
        string json = "{ \"key\": \"value\" }";
        var expression = new EqualExpression(new ConstantExpression(json), new ConstantExpression(JToken.Parse(json)));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_JTokenWithDifferentOrders_ReturnsTrue()
    {
        string json1 = "{ \"key1\": \"value1\", \"key2\": \"value2\" }";
        string json2 = "{ \"key2\": \"value2\", \"key1\": \"value1\" }";
        var token1 = JToken.Parse(json1);
        var token2 = JToken.Parse(json2);
        var expression = new EqualExpression(new ConstantExpression(token1), new ConstantExpression(token2));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_IntFromJsonPathToConstant()
    {
        var expression = new EqualExpression(new ConstantExpression(1), new JsonPathExpression("$var:jsonValue.intValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void Evaluate_BoolFromJsonPathToConstant()
    {
        var expression = new EqualExpression(new ConstantExpression(true), new JsonPathExpression("$var:jsonValue.boolValue"));
        var result = expression.Evaluate(_testContext);
        Assert.IsTrue((bool)result);
    }

    [Test]
    public void ToString_ReturnsExpectedFormat()
    {
        var expression = new EqualExpression(new ConstantExpression("Hello"), new ConstantExpression("World"));
        expression.Evaluate(_testContext);
        var toStringResult = expression.ToString();
        StringAssert.Contains("Equal", toStringResult);
        StringAssert.Contains("Hello", toStringResult);
        StringAssert.Contains("World", toStringResult);
    }
}
