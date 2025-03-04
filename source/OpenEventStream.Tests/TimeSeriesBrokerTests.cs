namespace OpenEventStream.Tests;

public class TimeSeriesBrokerTests
{
    private readonly Mock<ITimestampProvider> _mockTimestampProvider;
    private readonly Mock<ITimedLock> _mockTimedLock;
    private readonly TimeSeriesOptions _cacheOptions;
    private readonly TimeSeriesBroker<string> _timeSeriesBroker;

    public TimeSeriesBrokerTests()
    {
        _cacheOptions = new();
        _mockTimestampProvider = new Mock<ITimestampProvider>();
        _mockTimedLock = new Mock<ITimedLock>();
        _mockTimedLock.Setup(tl => tl.TryExecute(It.IsAny<Action>(), It.IsAny<TimeSpan?>(), It.IsAny<object?>()))
            .Callback<Action, TimeSpan?, object?>((action, timeout, syncLock) => action())
            .Returns(true);
        var timeSeriesFactory = new TimeSeriesFactory(_mockTimedLock.Object, _mockTimestampProvider.Object);
        _timeSeriesBroker = new TimeSeriesBroker<string>(_cacheOptions, timeSeriesFactory);
    }

    [Theory]
    [InlineData("Topic A", "Record 1A", "Record 2A", "Topic B", "Record 1B")]
    public void Test_AddAndRemoveRecords(string topicA, string record1A, string record2A, string topicB, string record1B)
    {
        CacheServiceTests.SetupTimestampProvider(_mockTimestampProvider);

        // Adding records to different topics
        _timeSeriesBroker.TryAdd(topicA, record1A);
        _timeSeriesBroker.TryAdd(topicA, record2A);
        _timeSeriesBroker.TryAdd(topicB, record1B);

        // Check and remove old records with a threshold of 2 seconds
        var removedRecordsA = _timeSeriesBroker.RemoveExpired(topicA, TimeSpan.FromSeconds(2));
        var removedRecordsB = _timeSeriesBroker.RemoveExpired(topicB, TimeSpan.FromSeconds(2));

        // Assert removed records
        Assert.Contains(record1A, removedRecordsA);
        Assert.DoesNotContain(record1B, removedRecordsB);
    }

    [Theory]
    [InlineData("Topic A", "Record 1A", "Record 2A", "Topic B", "Record 1B")]
    public void Test_DisplayRecords(string topicA, string record1A, string record2A, string topicB, string record1B)
    {
        var options = new TimeSeriesOptions();
        var _timeSeriesBroker = new TimeSeriesBroker<string>(options);

        // Adding records to different topics
        _timeSeriesBroker.TryAdd(topicA, record1A);
        _timeSeriesBroker.TryAdd(topicA, record2A);
        _timeSeriesBroker.TryAdd(topicB, record1B);

        // Display records
        var recordsA = _timeSeriesBroker.GetAllRecords(topicA);
        var recordsB = _timeSeriesBroker.GetAllRecords(topicB);

        // Assert records
        Assert.Equal(new[] { record1A, record2A }, recordsA);
        Assert.Equal(new[] { record1B }, recordsB);
    }
}
