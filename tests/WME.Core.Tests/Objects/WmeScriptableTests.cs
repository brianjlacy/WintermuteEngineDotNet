namespace WME.Core.Tests.Objects;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Objects;
using WME.Core.Scripting;
using Xunit;

/// <summary>
/// Tests for the WmeScriptable class.
/// </summary>
public class WmeScriptableTests
{
    private class TestScriptable : WmeScriptable
    {
        public TestScriptable(ILogger logger) : base(logger)
        {
        }

        public int TestProperty { get; set; } = 42;

        protected override WmeValue? GetBuiltInProperty(string name)
        {
            if (name.Equals("TestProperty", StringComparison.OrdinalIgnoreCase))
                return new WmeValue(TestProperty);

            return base.GetBuiltInProperty(name);
        }

        protected override bool SetBuiltInProperty(string name, WmeValue value)
        {
            if (name.Equals("TestProperty", StringComparison.OrdinalIgnoreCase))
            {
                TestProperty = Convert.ToInt32(value.Value);
                return true;
            }

            return base.SetBuiltInProperty(name, value);
        }

        protected override WmeValue? CallBuiltInMethod(string name, params WmeValue[] args)
        {
            if (name.Equals("TestMethod", StringComparison.OrdinalIgnoreCase))
            {
                var sum = args.Sum(a => Convert.ToInt32(a.Value));
                return new WmeValue(sum);
            }

            return base.CallBuiltInMethod(name, args);
        }
    }

    [Fact]
    public void GetProperty_ShouldReturnBuiltInProperty()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var value = obj.GetProperty("TestProperty");

        // Assert
        Assert.NotNull(value);
        Assert.Equal(42, value.Value);
    }

    [Fact]
    public void GetProperty_ShouldReturnCustomProperty()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);
        obj.SetProperty("CustomProp", new WmeValue("test"));

        // Act
        var value = obj.GetProperty("CustomProp");

        // Assert
        Assert.NotNull(value);
        Assert.Equal("test", value.Value);
    }

    [Fact]
    public void GetProperty_ShouldReturnNullForUnknownProperty()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var value = obj.GetProperty("UnknownProp");

        // Assert
        Assert.Null(value);
    }

    [Fact]
    public void SetProperty_ShouldSetBuiltInProperty()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var result = obj.SetProperty("TestProperty", new WmeValue(100));

        // Assert
        Assert.True(result);
        Assert.Equal(100, obj.TestProperty);
    }

    [Fact]
    public void SetProperty_ShouldSetCustomProperty()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var result = obj.SetProperty("CustomProp", new WmeValue("value"));

        // Assert
        Assert.True(result);
        var value = obj.GetProperty("CustomProp");
        Assert.NotNull(value);
        Assert.Equal("value", value.Value);
    }

    [Fact]
    public void CallMethod_ShouldCallBuiltInMethod()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var result = obj.CallMethod("TestMethod",
            new WmeValue(10),
            new WmeValue(20),
            new WmeValue(30));

        // Assert
        Assert.NotNull(result);
        Assert.Equal(60, result.Value);
    }

    [Fact]
    public void CallMethod_ShouldReturnNullForUnknownMethod()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var result = obj.CallMethod("UnknownMethod");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RegisterScriptMethod_ShouldAllowMethodCalling()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        Func<int, int, int> multiply = (a, b) => a * b;
        obj.RegisterScriptMethod("Multiply", multiply);

        // Act
        var result = obj.CallMethod("Multiply", 5, 7);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(35, result.Value);
    }

    [Fact]
    public void GetProperty_ShouldBeCaseInsensitive()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var value1 = obj.GetProperty("testproperty");
        var value2 = obj.GetProperty("TESTPROPERTY");
        var value3 = obj.GetProperty("TestProperty");

        // Assert
        Assert.NotNull(value1);
        Assert.NotNull(value2);
        Assert.NotNull(value3);
        Assert.Equal(value1.Value, value2.Value);
        Assert.Equal(value1.Value, value3.Value);
    }

    [Fact]
    public void CallMethod_ShouldBeCaseInsensitive()
    {
        // Arrange
        var logger = new Mock<ILogger<TestScriptable>>();
        var obj = new TestScriptable(logger.Object);

        // Act
        var result1 = obj.CallMethod("testmethod", new WmeValue(10));
        var result2 = obj.CallMethod("TESTMETHOD", new WmeValue(10));
        var result3 = obj.CallMethod("TestMethod", new WmeValue(10));

        // Assert
        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotNull(result3);
        Assert.Equal(result1.Value, result2.Value);
        Assert.Equal(result1.Value, result3.Value);
    }
}
