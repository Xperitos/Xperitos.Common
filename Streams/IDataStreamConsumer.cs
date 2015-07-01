namespace Xperitos.Common.Streams
{
    public interface IDataStreamConsumer
    {
        /// <summary>
        /// Queue the data.
        /// </summary>
        void QueueData(byte[] data);
    }
}