using Xunit;

namespace OpenEventStream.Tests;

public class CacheServiceTests
{
    private readonly Mock<ITimestampProvider> _mockTimestampProvider;
    private readonly Mock<ITimedLock> _mockTimedLock;
    private readonly CacheOptions _cacheOptions;
    private readonly CacheService<string> _cacheService;

    public CacheServiceTests()
    {
        _mockTimestampProvider = new Mock<ITimestampProvider>();
        _mockTimedLock = new Mock<ITimedLock>();
        _mockTimedLock.Setup(tl => tl.TryExecute(It.IsAny<Action>(), It.IsAny<TimeSpan?>(), It.IsAny<object?>()))
            .Callback<Action, TimeSpan?, object?>((action, timeout, syncLock) => action())
            .Returns(true);
        _cacheOptions = new CacheOptions
        {
            Expiry = TimeSpan.FromMinutes(1),
            Timeout = TimeSpan.FromSeconds(1),
            UseCompositeKey = false,
            Delimiter = ":"
        };
        _cacheService = new CacheService<string>(_cacheOptions, _mockTimedLock.Object, _mockTimestampProvider.Object);
    }

    /// <summary>
    /// Setup the timestamp provider to return a sequence of seconds.
    /// Default is one second intervals from zero for nine seconds.
    /// </summary>
    internal static void SetupTimestampProvider(Mock<ITimestampProvider> mockTimestampProvider, int seconds = 10, IEnumerable<long>? ticks = null)
    {
        ticks ??= Enumerable.Range(0, seconds).Select(s => s * TimeSpan.TicksPerSecond);
        var setupSequence = mockTimestampProvider.SetupSequence(tp => tp.Ticks);
        foreach (var item in ticks)
        {
            setupSequence = setupSequence.Returns(item);
        }
    }

    [Fact]
    public void GetOrAdd_ShouldAddNewItem_WhenKeyDoesNotExist()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        _mockTimestampProvider.Setup(tp => tp.Ticks).Returns(1 * TimeSpan.TicksPerSecond);

        // Act
        var result = _cacheService.GetOrAdd(key, k => value);

        // Assert
        Assert.Equal(1, _cacheService.Count);
        Assert.Equal(value, result);
    }

    [Fact]
    public void GetOrAdd_ShouldReturnExistingItem_WhenKeyExists()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        _mockTimestampProvider.Setup(tp => tp.Ticks).Returns(1 * TimeSpan.TicksPerSecond);
        _cacheService.GetOrAdd(key, k => value);

        // Act
        var result = _cacheService.GetOrAdd(key, k => "newValue");

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void Flush_ShouldRemoveExpiredItems()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        // 1 second intervals for initial add, check expiry, and after expiry
        SetupTimestampProvider(_mockTimestampProvider);
        _cacheService.GetOrAdd(key, k => value);

        // Act
        var expiredKeys = _cacheService.Flush(TimeSpan.FromSeconds(1));

        // Assert
        Assert.Contains(key, expiredKeys);
    }

    [Fact]
    public void Flush_ShouldNotRemoveNonExpiredItems()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        // 1 second intervals for initial add, check expiry
        SetupTimestampProvider(_mockTimestampProvider);
        _cacheService.GetOrAdd(key, k => value);

        // Act
        var expiredKeys = _cacheService.Flush(TimeSpan.FromSeconds(2));

        // Assert
        Assert.DoesNotContain(key, expiredKeys);
    }

    [Fact]
    public void Dispose_ShouldRemoveAllItems()
    {
        // Arrange
        var key = "testKey";
        var value = "testValue";
        _cacheService.GetOrAdd(key, k => value);

        // Act
        _cacheService.Dispose();

        // Assert
        Assert.Equal(0, _cacheService.Count);
    }
}
