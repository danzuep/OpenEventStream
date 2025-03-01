using OpenEventStream.Models;
using OpenEventStream.Services;

class Program
{
    static void Main()
    {
        var options = new TimeSeriesOptions();
        var timeSeriesBroker = new TimeSeriesBroker<string>(options);

        Console.WriteLine("Add CSV records (press enter for demonstration defaults):");
        var records = Console.ReadLine();
        Console.WriteLine();

        if (string.IsNullOrWhiteSpace(records))
        {
            records = "Record 1,Record 2,Record 3";
        }
        foreach (var value in records.Split(','))
        {
            timeSeriesBroker.TryAdd(value);
        }

        Console.WriteLine("Add more CSV records if desired:");
        records = Console.ReadLine();
        Console.WriteLine();

        if (!string.IsNullOrWhiteSpace(records))
        {
            foreach (var value in records.Split(','))
            {
                timeSeriesBroker.TryAdd(value);
            }
        }

        Console.WriteLine("Records added:");
        foreach (var item in timeSeriesBroker.GetAllRecords())
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();

        Console.WriteLine("Seconds of data to keep (press enter to delete all):");
        var input = Console.ReadLine();
        Console.WriteLine();

        if (!int.TryParse(input, out var seconds))
        {
            seconds = 0;
        }
        Console.WriteLine("Records removed:");
        foreach (var item in timeSeriesBroker.RemoveExpired(TimeSpan.FromSeconds(seconds)))
        {
            Console.WriteLine(item);
        }
        Console.WriteLine();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}