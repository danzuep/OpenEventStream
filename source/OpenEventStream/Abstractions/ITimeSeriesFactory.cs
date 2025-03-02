namespace OpenEventStream.Abstractions
{
    public interface ITimeSeriesFactory
    {
        ITimeSeries<T> Create<T>();
    }
}