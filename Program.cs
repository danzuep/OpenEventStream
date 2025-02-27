using OpenEventStream.Models;
using OpenEventStream.Services;

class Program
{
    static void Main()
    {
        var options = new TimeSeriesOptions();
        var timeSeriesBroker = new TimeSeriesBroker<string>(options);

        // Adding records to different topics
        timeSeriesBroker.TryAdd("Topic A", "Record 1A");
        Thread.Sleep(2000);
        timeSeriesBroker.TryAdd("Topic A", "Record 2A");
        Thread.Sleep(2000);
        timeSeriesBroker.TryAdd("Topic B", "Record 1B");

        // Display records before checking
        Console.WriteLine("Records before checking:");
        DisplayRecords(timeSeriesBroker, "Topic A");
        DisplayRecords(timeSeriesBroker, "Topic B");

        // Check and remove old records with a threshold of 5 seconds
        Thread.Sleep(3000); // Wait for some records to expire
        var removedRecordsA = timeSeriesBroker.RemoveExpired("Topic A", TimeSpan.FromSeconds(5));
        var removedRecordsB = timeSeriesBroker.RemoveExpired("Topic B", TimeSpan.FromSeconds(5));

        // Display removed records
        Console.WriteLine("Removed records from Topic A:");
        foreach (var record in removedRecordsA)
        {
            Console.WriteLine(record);
        }

        Console.WriteLine("Removed records from Topic B:");
        foreach (var record in removedRecordsB)
        {
            Console.WriteLine(record);
        }

        // Display records after checking
        Console.WriteLine("Records after checking:");
        DisplayRecords(timeSeriesBroker, "Topic A");
        DisplayRecords(timeSeriesBroker, "Topic B");

        Console.ReadLine();
    }

    private static void DisplayRecords<T>(TimeSeriesBroker<T> broker, string topic)
    {
        Console.WriteLine($"Records for topic '{topic}':");
        if (broker.TryGetTimeSeries(topic, out var partition))
        {
            foreach (var record in partition)
            {
                Console.WriteLine($"{record.Key}: {record.Value}");
            }
        }
    }
}