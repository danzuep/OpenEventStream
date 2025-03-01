using System.Collections.Concurrent;

namespace OpenEventStream.Tests;

public class TimeSeriesBrokerTests
{
    [Fact]
    public void Test_AddAndRemoveRecords()
    {
        var options = new TimeSeriesOptions();
        var timeSeriesBroker = new TimeSeriesBroker<string>(options);

        // Adding records to different topics
        timeSeriesBroker.TryAdd("Topic A", "Record 1A");
        Thread.Sleep(2000);
        timeSeriesBroker.TryAdd("Topic A", "Record 2A");
        Thread.Sleep(2000);
        timeSeriesBroker.TryAdd("Topic B", "Record 1B");

        // Check and remove old records with a threshold of 5 seconds
        Thread.Sleep(3000); // Wait for some records to expire
        var removedRecordsA = timeSeriesBroker.RemoveExpired("Topic A", TimeSpan.FromSeconds(5));
        var removedRecordsB = timeSeriesBroker.RemoveExpired("Topic B", TimeSpan.FromSeconds(5));

        // Assert removed records
        Assert.Contains("Record 1A", removedRecordsA);
        Assert.Contains("Record 2A", removedRecordsA);
        Assert.DoesNotContain("Record 1B", removedRecordsB);
    }

    [Fact]
    public void Test_DisplayRecords()
    {
        var options = new TimeSeriesOptions();
        var timeSeriesBroker = new TimeSeriesBroker<string>(options);

        // Adding records to different topics
        timeSeriesBroker.TryAdd("Topic A", "Record 1A");
        timeSeriesBroker.TryAdd("Topic A", "Record 2A");
        timeSeriesBroker.TryAdd("Topic B", "Record 1B");

        // Display records
        var recordsA = timeSeriesBroker.GetAllRecords("Topic A");
        var recordsB = timeSeriesBroker.GetAllRecords("Topic B");

        // Assert records
        Assert.Contains("Record 1A", recordsA);
        Assert.Contains("Record 2A", recordsA);
        Assert.Contains("Record 1B", recordsB);
    }
}
