namespace Xperitos.Common.Streams
{
    interface IDataStreamConsumer
    {
        /// <summary>
        /// Queue the data.
        /// </summary>
        void QueueData(byte[] data);
    }
}