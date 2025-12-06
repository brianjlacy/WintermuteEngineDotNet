namespace WME.Core.Tests.Objects;

using Microsoft.Extensions.Logging;
using Moq;
using WME.Core.Objects;
using Xunit;

/// <summary>
/// Tests for the WmeObject class.
/// </summary>
public class WmeObjectTests
{
    private class TestObject : WmeObject
    {
        public TestObject(ILogger logger) : base(logger)
        {
        }

        public int UpdateCallCount { get; private set; }
        public int RenderCallCount { get; private set; }

        public override bool Update(TimeSpan deltaTime)
        {
            UpdateCallCount++;
            return base.Update(deltaTime);
        }

        public override bool Render()
        {
            RenderCallCount++;
            return base.Render();
        }
    }

    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();

        // Act
        var obj = new TestObject(logger.Object);

        // Assert
        Assert.True(obj.Visible);
        Assert.True(obj.Active);
        Assert.Equal(0, obj.Priority);
    }

    [Fact]
    public void Visible_ShouldBeSettable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        obj.Visible = false;

        // Assert
        Assert.False(obj.Visible);
    }

    [Fact]
    public void Active_ShouldBeSettable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        obj.Active = false;

        // Assert
        Assert.False(obj.Active);
    }

    [Fact]
    public void Priority_ShouldBeSettable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        obj.Priority = 100;

        // Assert
        Assert.Equal(100, obj.Priority);
    }

    [Fact]
    public void Update_ShouldReturnActiveState()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act & Assert (active)
        Assert.True(obj.Update(TimeSpan.FromSeconds(0.016)));

        // Act & Assert (inactive)
        obj.Active = false;
        Assert.False(obj.Update(TimeSpan.FromSeconds(0.016)));
    }

    [Fact]
    public void Update_ShouldBeCallable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        obj.Update(TimeSpan.FromSeconds(0.016));
        obj.Update(TimeSpan.FromSeconds(0.016));

        // Assert
        Assert.Equal(2, obj.UpdateCallCount);
    }

    [Fact]
    public void Render_ShouldReturnVisibleState()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act & Assert (visible)
        Assert.True(obj.Render());

        // Act & Assert (invisible)
        obj.Visible = false;
        Assert.False(obj.Render());
    }

    [Fact]
    public void Render_ShouldBeCallable()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        obj.Render();
        obj.Render();

        // Assert
        Assert.Equal(2, obj.RenderCallCount);
    }

    [Fact]
    public void Update_ShouldThrowWhenDisposed()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);
        obj.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => obj.Update(TimeSpan.FromSeconds(0.016)));
    }

    [Fact]
    public void Render_ShouldThrowWhenDisposed()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);
        obj.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => obj.Render());
    }

    [Fact]
    public void PropertyChanges_ShouldTriggerOnChanged()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);
        int changedCallCount = 0;

        obj.PropertyChanged += (sender, e) =>
        {
            changedCallCount++;
        };

        // Act
        obj.Visible = false;
        obj.Active = false;
        obj.Priority = 50;

        // Assert
        Assert.Equal(3, changedCallCount);
    }

    [Fact]
    public void CompareTo_ShouldCompareByPriority()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj1 = new TestObject(logger.Object) { Priority = 100 };
        var obj2 = new TestObject(logger.Object) { Priority = 200 };
        var obj3 = new TestObject(logger.Object) { Priority = 100 };

        // Act & Assert
        Assert.True(obj1.CompareTo(obj2) < 0); // obj1 < obj2
        Assert.True(obj2.CompareTo(obj1) > 0); // obj2 > obj1
        Assert.Equal(0, obj1.CompareTo(obj3)); // obj1 == obj3
    }

    [Fact]
    public void CompareTo_ShouldHandleNull()
    {
        // Arrange
        var logger = new Mock<ILogger<TestObject>>();
        var obj = new TestObject(logger.Object);

        // Act
        var result = obj.CompareTo(null);

        // Assert
        Assert.Equal(1, result); // Non-null is greater than null
    }
}
