namespace RoutePlanning
{
    /// <summary>
    /// An object that implements this is one that will save data to the device for later retrival
    /// </summary>
    /// <remarks>
    /// In computing, a cache is a hardware or software component that stores data so that future requests for that data can be served faster; the data stored in a cache might be the result of an earlier computation or a copy of data stored elsewhere
    /// </remarks>
    public interface ICachedData
    {
        FileInfo? FileLocation { get; }
    }
}
