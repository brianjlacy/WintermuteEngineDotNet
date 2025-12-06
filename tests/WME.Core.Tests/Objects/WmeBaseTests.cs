namespace WME.Core.Tests.Objects;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Objects;
using Xunit;

/// <summary>
/// Tests for the WmeBase class.
/// </summary>
public class WmeBaseTests
{
    private class TestWmeBase : WmeBase
    {
        public TestWmeBase(ILogger logger) : base(logger)
        {
        }
    }

    [Fact]
    public void Constructor_ShouldAssignUniqueId()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();

        // Act
        var obj1 = new TestWmeBase(logger.Object);
        var obj2 = new TestWmeBase(logger.Object);

        // Assert
        Assert.NotEqual(0, obj1.Id);
        Assert.NotEqual(0, obj2.Id);
        Assert.NotEqual(obj1.Id, obj2.Id);
    }

    [Fact]
    public void Constructor_ShouldRequireLogger()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TestWmeBase(null!));
    }

    [Fact]
    public void Name_ShouldBeNullByDefault()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();

        // Act
        var obj = new TestWmeBase(logger.Object);

        // Assert
        Assert.Null(obj.Name);
    }

    [Fact]
    public void Name_ShouldBeSettable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object);

        // Act
        obj.Name = "TestObject";

        // Assert
        Assert.Equal("TestObject", obj.Name);
    }

    [Fact]
    public void IsDisposed_ShouldBeFalseByDefault()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();

        // Act
        var obj = new TestWmeBase(logger.Object);

        // Assert
        Assert.False(obj.IsDisposed);
    }

    [Fact]
    public void Dispose_ShouldSetIsDisposedToTrue()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object);

        // Act
        obj.Dispose();

        // Assert
        Assert.True(obj.IsDisposed);
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object);

        // Act
        obj.Dispose();
        obj.Dispose();

        // Assert
        Assert.True(obj.IsDisposed);
    }

    [Fact]
    public void Persist_ShouldReturnTrue()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object)
        {
            Name = "TestObject"
        };

        var persistMgr = new Mock<IPersistenceManager>();
        string? persistedName = null;
        persistMgr.Setup(p => p.Transfer(nameof(WmeBase.Name), ref It.Ref<string?>.IsAny))
            .Callback((string key, ref string? value) =>
            {
                persistedName = value;
            });

        // Act
        var result = obj.Persist(persistMgr.Object);

        // Assert
        Assert.True(result);
        persistMgr.Verify(p => p.Transfer(nameof(WmeBase.Name), ref It.Ref<string?>.IsAny), Times.Once);
    }

    [Fact]
    public void ToString_ShouldIncludeTypeAndId()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object);

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Contains("TestWmeBase", result);
        Assert.Contains(obj.Id.ToString(), result);
    }

    [Fact]
    public void ToString_ShouldIncludeNameWhenSet()
    {
        // Arrange
        var logger = new Mock<ILogger<TestWmeBase>>();
        var obj = new TestWmeBase(logger.Object)
        {
            Name = "MyObject"
        };

        // Act
        var result = obj.ToString();

        // Assert
        Assert.Contains("MyObject", result);
    }
}
