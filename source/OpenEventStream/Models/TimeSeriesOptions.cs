namespace OpenEventStream.Models;

public class TimeSeriesOptions //: IOptions<TimeSeriesOptions>
{
    public string DefaultTopic { get; set; } = "Default";

    //TimeSeriesOptions IOptions<TimeSeriesOptions>.Value => this;
}