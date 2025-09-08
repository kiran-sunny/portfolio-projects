# API Reference Documentation
## Enterprise YAML-Based Testing Framework

This document provides comprehensive API reference for developers and architects who need to understand or extend the framework's capabilities.

---

## 📚 Core API Reference

### TestStep Interface Hierarchy

```csharp
/// <summary>
/// Core abstraction for all test step implementations
/// </summary>
public interface ITestStepExecutor
{
    /// <summary>
    /// Executes a test step with the given input and context
    /// </summary>
    /// <param name="input">Step input parameters</param>
    /// <param name="context">Test execution context</param>
    /// <returns>Step execution output</returns>
    ITestStepOutput Execute(ITestStepInput input, TestContext context);
}

/// <summary>
/// Base implementation providing retry logic and error handling
/// </summary>
public abstract class BaseTestStepExecutor<T> : ITestStepExecutor 
    where T : TestStepOutputSettings
{
    protected T OutputSettings { get; }
    protected TestStepExecutionSettings ExecutionSettings { get; }
    
    /// <summary>
    /// Template method for step execution with built-in retry logic
    /// </summary>
    public ITestStepOutput Execute(ITestStepInput input, TestContext context)
    {
        // Retry logic implementation
        for (int attempt = 1; attempt <= ExecutionSettings.MaxRetries; attempt++)
        {
            try
            {
                var result = ExecuteAttempt(input, context);
                if (ShouldRetry(result, attempt)) continue;
                return result;
            }
            catch (Exception ex) when (ShouldRetryOnException(ex, attempt))
            {
                if (attempt == ExecutionSettings.MaxRetries) throw;
                Thread.Sleep(ExecutionSettings.RetryDelayMs);
            }
        }
    }
    
    /// <summary>
    /// Abstract method for concrete step implementation
    /// </summary>
    protected abstract ITestStepOutput ExecuteAttempt(ITestStepInput input, TestContext context);
    
    /// <summary>
    /// Determines if step should be retried based on output
    /// </summary>
    protected virtual bool ShouldRetry(ITestStepOutput output, int attempt) => false;
    
    /// <summary>
    /// Determines if step should be retried based on exception
    /// </summary>
    protected virtual bool ShouldRetryOnException(Exception ex, int attempt) => true;
}
```

### TestContext API

```csharp
/// <summary>
/// Central context for test execution, managing variables, tables, and data flow
/// </summary>
public class TestContext
{
    /// <summary>
    /// Variable management with scoping support
    /// </summary>
    public VariableManager Variables { get; }
    
    /// <summary>
    /// Table management for data-driven testing
    /// </summary>
    public TableManager Tables { get; }
    
    /// <summary>
    /// Utility functions for context operations
    /// </summary>
    public ContextUtils Utils { get; }
    
    /// <summary>
    /// Root JSON object containing all test data
    /// </summary>
    public JObject Json { get; }
    
    /// <summary>
    /// Resolves variables and expressions in context
    /// </summary>
    /// <typeparam name="T">Target type for resolution</typeparam>
    /// <param name="value">Value to resolve</param>
    /// <returns>Resolved value of type T</returns>
    public T Resolve<T>(object value);
    
    /// <summary>
    /// Adds step input to context for reference by subsequent steps
    /// </summary>
    /// <param name="stepId">Unique step identifier</param>
    /// <param name="input">Step input data</param>
    public void AddStepInput(string stepId, JToken input);
    
    /// <summary>
    /// Adds step output to context for reference by subsequent steps
    /// </summary>
    /// <param name="stepId">Unique step identifier</param>
    /// <param name="output">Step output data</param>
    public void AddStepOutput(string stepId, JToken output);
    
    /// <summary>
    /// Sets a specific value within a step's context
    /// </summary>
    /// <param name="stepId">Step identifier</param>
    /// <param name="key">Value key</param>
    /// <param name="value">Value to set</param>
    public void SetValueToStep(string stepId, string key, JToken value);
}
```

### Assertion Framework API

```csharp
/// <summary>
/// Base class for all assertion implementations
/// </summary>
public abstract class Asserter
{
    protected IExpression Expression { get; }
    
    /// <summary>
    /// Executes the assertion against the current context
    /// </summary>
    /// <param name="context">Test context</param>
    /// <param name="result">Step execution result</param>
    public void Test(TestContext context, TestStep.ExecutionResult result)
    {
        if (!(bool)Expression.Evaluate(context))
        {
            OnTestFailure(context, result);
        }
    }
    
    /// <summary>
    /// Called when assertion fails - implemented by concrete asserters
    /// </summary>
    protected abstract void OnTestFailure(TestContext context, TestStep.ExecutionResult result);
}

/// <summary>
/// Expression evaluation interface for dynamic assertions
/// </summary>
public interface IExpression
{
    /// <summary>
    /// Evaluates the expression in the given context
    /// </summary>
    /// <param name="context">Test context for evaluation</param>
    /// <returns>Evaluation result</returns>
    object Evaluate(TestContext context);
    
    /// <summary>
    /// Gets human-readable error message for failed assertions
    /// </summary>
    /// <returns>Error message</returns>
    string GetErrorMessage();
}
```

---

## 🔧 Extension Points

### Creating Custom Step Types

#### 1. Implement the Executor

```csharp
/// <summary>
/// Example: Custom database step executor
/// </summary>
public class DatabaseTestStepExecutor : BaseTestStepExecutor<DatabaseOutputSettings>
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public DatabaseTestStepExecutor(
        DatabaseOutputSettings outputSettings,
        TestStepExecutionSettings executionSettings,
        IDbConnectionFactory connectionFactory) 
        : base(outputSettings, executionSettings)
    {
        _connectionFactory = connectionFactory;
    }
    
    protected override ITestStepOutput ExecuteAttempt(ITestStepInput input, TestContext context)
    {
        var dbInput = (DatabaseStepInput)input;
        
        using var connection = _connectionFactory.CreateConnection(dbInput.ConnectionString);
        connection.Open();
        
        using var command = connection.CreateCommand();
        command.CommandText = dbInput.Query;
        command.CommandTimeout = ExecutionSettings.RequestTimeoutMs / 1000;
        
        var startTime = DateTime.UtcNow;
        var result = command.ExecuteScalar();
        var endTime = DateTime.UtcNow;
        
        return new DatabaseStepOutput
        {
            Result = result,
            ExecutionTime = endTime - startTime,
            Settings = OutputSettings
        };
    }
}
```

#### 2. Define Input/Output Models

```csharp
/// <summary>
/// Database step input model
/// </summary>
public class DatabaseStepInput : ITestStepInput
{
    public string ConnectionString { get; set; }
    public string Query { get; set; }
    public Dictionary<string, object> Parameters { get; set; }
    
    public JToken ToJson() => JToken.FromObject(this);
}

/// <summary>
/// Database step output model
/// </summary>
public class DatabaseStepOutput : ITestStepOutput
{
    public object Result { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public TestStepOutputSettings Settings { get; set; }
    
    public JToken ToJson() => JToken.FromObject(new
    {
        result = Result,
        executionTimeMs = ExecutionTime.TotalMilliseconds
    });
}
```

#### 3. Register the Step Type

```csharp
/// <summary>
/// Factory registration for custom step types
/// </summary>
public static class CustomStepRegistration
{
    public static void RegisterCustomSteps()
    {
        TestStepFactory.RegisterStepType("database", (stepDefinition) =>
        {
            var outputSettings = stepDefinition["Output"]?.ToObject<DatabaseOutputSettings>() 
                ?? new DatabaseOutputSettings();
            var executionSettings = stepDefinition["Execution"]?.ToObject<TestStepExecutionSettings>() 
                ?? new TestStepExecutionSettings();
            
            return new DatabaseTestStepExecutor(outputSettings, executionSettings, new DbConnectionFactory());
        });
    }
}
```

### Creating Custom Assertions

```csharp
/// <summary>
/// Example: Custom regex assertion
/// </summary>
public class RegexAssertion : Asserter
{
    public RegexAssertion(IExpression expression) : base(expression) { }
    
    protected override void OnTestFailure(TestContext context, TestStep.ExecutionResult result)
    {
        var errorMessage = $"Regex assertion failed: {Expression.GetErrorMessage()}";
        TestLogger.LogError(errorMessage);
        result.AddAssertionFailure(errorMessage);
        throw new AssertionException(errorMessage);
    }
}

/// <summary>
/// Regex expression implementation
/// </summary>
public class RegexExpression : IExpression
{
    private readonly string _pattern;
    private readonly IExpression _valueExpression;
    
    public RegexExpression(string pattern, IExpression valueExpression)
    {
        _pattern = pattern;
        _valueExpression = valueExpression;
    }
    
    public object Evaluate(TestContext context)
    {
        var value = _valueExpression.Evaluate(context)?.ToString() ?? "";
        return Regex.IsMatch(value, _pattern);
    }
    
    public string GetErrorMessage()
    {
        return $"Value does not match pattern: {_pattern}";
    }
}
```

### Creating Custom Output Parsers

```csharp
/// <summary>
/// Custom XML output parser
/// </summary>
public class XmlOutputParser : IOutputParser
{
    public JObject Parse(string output, ParserConfiguration config)
    {
        try
        {
            var xmlDoc = XDocument.Parse(output);
            var result = new JObject();
            
            foreach (var pattern in config.Patterns)
            {
                var elements = xmlDoc.XPathSelectElements(pattern.Pattern);
                var values = elements.Select(e => ConvertValue(e.Value, pattern.Type)).ToArray();
                
                if (values.Length == 1)
                    result[pattern.Field] = JToken.FromObject(values[0]);
                else
                    result[pattern.Field] = JArray.FromObject(values);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new ParsingException($"Failed to parse XML output: {ex.Message}", ex);
        }
    }
    
    private object ConvertValue(string value, string type)
    {
        return type.ToLower() switch
        {
            "int" => int.Parse(value),
            "double" => double.Parse(value),
            "bool" => bool.Parse(value),
            "datetime" => DateTime.Parse(value),
            _ => value
        };
    }
}
```

---

## 📊 Performance Optimization Guide

### Memory Management Best Practices

```csharp
/// <summary>
/// Efficient JSON processing for large datasets
/// </summary>
public class OptimizedJsonProcessor
{
    private readonly JsonSerializer _serializer = new JsonSerializer();
    
    /// <summary>
    /// Streams large JSON arrays without loading entire content into memory
    /// </summary>
    public async IAsyncEnumerable<T> StreamJsonArray<T>(Stream jsonStream)
    {
        using var reader = new JsonTextReader(new StreamReader(jsonStream));
        
        // Navigate to array start
        while (await reader.ReadAsync() && reader.TokenType != JsonToken.StartArray) { }
        
        // Process array elements one by one
        while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndArray)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                yield return _serializer.Deserialize<T>(reader);
            }
        }
    }
}

/// <summary>
/// Connection pooling for HTTP clients
/// </summary>
public class HttpClientPool : IDisposable
{
    private readonly ConcurrentQueue<HttpClient> _availableClients = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _maxPoolSize;
    
    public HttpClientPool(int maxPoolSize = 10)
    {
        _maxPoolSize = maxPoolSize;
        _semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);
    }
    
    public async Task<PooledHttpClient> GetClientAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        if (_availableClients.TryDequeue(out var client))
        {
            return new PooledHttpClient(client, this);
        }
        
        return new PooledHttpClient(CreateNewClient(), this);
    }
    
    internal void ReturnClient(HttpClient client)
    {
        _availableClients.Enqueue(client);
        _semaphore.Release();
    }
    
    private HttpClient CreateNewClient()
    {
        return new HttpClient(new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            MaxConnectionsPerServer = 10
        });
    }
    
    public void Dispose()
    {
        while (_availableClients.TryDequeue(out var client))
        {
            client.Dispose();
        }
        _semaphore.Dispose();
    }
}
```

---

*This API reference provides comprehensive documentation for extending and optimizing the enterprise testing framework. The modular architecture enables extensive customization while maintaining performance and reliability.*
