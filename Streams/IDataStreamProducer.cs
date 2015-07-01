using System;

namespace Xperitos.Common.Streams
{
    public interface IDataStreamProducer : IDisposable
    {
        /// <summary>
        /// Start producing data.
        /// </summary>
        void Start();

        /// <summary>
        /// The data stream.
        /// </summary>
        IObservable<byte> DataStream { get; }
    }
}