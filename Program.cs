using OpenEventStream.Models;
using OpenEventStream;
using OpenEventStream.Abstractions;

class Program
{
    static void Main()
    {
        var options = new TimeSeriesOptions();
        var timeSeriesBroker = new TimeSeriesBroker<string>(options);

        // Adding records to different topics
        timeSeriesBroker.Add("Record 1A", "Topic A");
        Thread.Sleep(2000);
        timeSeriesBroker.Add("Record 2A", "Topic A");
        Thread.Sleep(2000);
        timeSeriesBroker.Add("Record 1B", "Topic B");

        // Display records before checking
        Console.WriteLine("Records before checking:");
        DisplayRecords(timeSeriesBroker, "Topic A");
        DisplayRecords(timeSeriesBroker, "Topic B");

        // Check and remove old records with a threshold of 5 seconds
        Thread.Sleep(3000); // Wait for some records to expire
        var removedRecordsA = timeSeriesBroker.RemoveExpired(TimeSpan.FromSeconds(5), "Topic A");
        var removedRecordsB = timeSeriesBroker.RemoveExpired(TimeSpan.FromSeconds(5), "Topic B");

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