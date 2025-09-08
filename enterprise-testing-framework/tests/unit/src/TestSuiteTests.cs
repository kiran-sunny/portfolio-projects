// Curated test from internal project; sanitized for portfolio use.
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestLib.Tests;

[TestFixture]
public class TestSuiteTests
{
    private string _testDirectory;

    [SetUp]
    public void SetUp()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        var yamlContent = @"
                              Name: 'root-test'
                              Description: 'A test without includes'
                              Steps:
                                - Name: 'Step 1'
                                  Description: 'First step description'
                                  Input:
                                    Method: 'GET'
                                    RequestUri: 'http://example.com/api'
                                    Headers:
                                      Content-Type: 'application/json'
                            ";
        File.WriteAllText(Path.Combine(_testDirectory, "root-test.yaml"), yamlContent);

        var subDir1 = Path.Combine(_testDirectory, "subdir1");
        var subDir2 = Path.Combine(_testDirectory, "subdir2", "nested");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        var yamlContent1 = @"
                              Name: 'subdir1-test'
                              Description: 'A test without includes'
                              Steps:
                                - Name: 'Step 1'
                                  Description: 'First step description'
                                  Input:
                                    Method: 'GET'
                                    RequestUri: 'http://example.com/api'
                                    Headers:
                                      Content-Type: 'application/json'
                            ";
        var yamlContent2 = @"
                              Name: 'nested-test'
                              Description: 'A test without includes'
                              Steps:
                                - Name: 'Step 1'
                                  Description: 'First step description'
                                  Input:
                                    Method: 'GET'
                                    RequestUri: 'http://example.com/api'
                                    Headers:
                                      Content-Type: 'application/json'
                            ";

        File.WriteAllText(Path.Combine(subDir1, "subdir1-test.yaml"), yamlContent1);
        File.WriteAllText(Path.Combine(subDir2, "nested-test.yaml"), yamlContent2);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }

    [Test]
    public void AddTest_ShouldLoadTestsFromAllDirectories()
    {
        var testSuite = new TestSuite();
        var pattern = "*-test.yaml";
        testSuite.AddTest(_testDirectory, pattern);

        var tests = testSuite.GetAllTests();
        Assert.AreEqual(3, tests.Count);
        Assert.IsTrue(tests.Any(t => t.Name == "root-test"));
        Assert.IsTrue(tests.Any(t => t.Name == "subdir1-test"));
        Assert.IsTrue(tests.Any(t => t.Name == "nested-test"));
    }

    [Test]
    public void AddTest_ShouldMatchSpecificSubdirectoryPattern()
    {
        var testSuite = new TestSuite();
        var pattern = "*/subdir1/*-test.yaml";
        testSuite.AddTest(_testDirectory, pattern);

        var tests = testSuite.GetAllTests();
        Assert.AreEqual(1, tests.Count);
        Assert.IsTrue(tests.Any(t => t.Name == "subdir1-test"));
        Assert.IsFalse(tests.Any(t => t.Name == "root-test"));
        Assert.IsFalse(tests.Any(t => t.Name == "nested-test"));
    }

    [Test]
    public void AddTest_ShouldHandleInvalidPatternGracefully()
    {
        var testSuite = new TestSuite();
        var pattern = "*.invalid-pattern";
        testSuite.AddTest(_testDirectory, pattern);

        var tests = testSuite.GetAllTests();
        Assert.AreEqual(0, tests.Count);
    }
}
