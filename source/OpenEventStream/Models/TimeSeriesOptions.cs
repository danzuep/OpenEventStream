namespace OpenEventStream.Models;

using Microsoft.Extensions.Options;

public class TimeSeriesOptions : IOptions<TimeSeriesOptions>
{
    public string DefaultTopic { get; set; } = "Default";

    TimeSeriesOptions IOptions<TimeSeriesOptions>.Value => this;
}