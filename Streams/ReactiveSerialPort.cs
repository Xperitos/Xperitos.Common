using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Splat;

namespace Xperitos.Common.Streams
{
    public class ReactiveSerialPort : IDataStreamConsumerProducer
    {
        public ReactiveSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake = Handshake.None)
        {
            m_serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
            m_serialPort.Handshake = handshake;

            m_serialPort.DataReceived += ( o, e ) => m_hasDataEvent.Set();
        }

        private readonly SerialPort m_serialPort;
        private readonly ManualResetEventSlim m_hasDataEvent = new ManualResetEventSlim();

        private readonly Subject<byte> m_dataReceivedSubject = new Subject<byte>();

        /// <summary>
        /// Start processing data from the serial port.
        /// </summary>
        public void Start()
        {
            if (m_cancellationToken != null && m_cancellationToken.IsCancellationRequested)
                throw new ObjectDisposedException("Object disposed");

            if (m_thread != null)
                throw new InvalidOperationException("Already started");

            m_cancellationToken = new CancellationTokenSource();
            m_thread = new Thread(() => ThreadProc(m_cancellationToken.Token))
                       {
                           IsBackground = true, 
                           Name = "SerialPort Thread"
                       };
            m_thread.Start();
        }

        /// <summary>
        /// Queue the specified data to send.
        /// </summary>
        public void QueueData(byte[] data)
        {
            lock(m_pendingTransmitData)
            {
                var totalQueuedData = m_pendingTransmitData.Sum(v => v.Length);
                if (totalQueuedData > 2048)
                    Log.Warning("Total queued data on serial port exceed 2048 bytes");

                m_pendingTransmitData.Enqueue(data);
                m_hasDataEvent.Set();
            }
        }

        private readonly Queue<byte[]> m_pendingTransmitData = new Queue<byte[]>(); 

        /// <summary>
        /// Use this to obtain received data stream. Called from the serial port thread.
        /// Errors are transmitted on this stream as well.
        /// </summary>
        public IObservable<byte> DataStream => m_dataReceivedSubject;

        private void ThreadProc(CancellationToken cancellation)
        {
            Log.Debug("Serial port thread started");

            try
            {
                do
                {
                    try
                    {
                        m_serialPort.Open();
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Failed to open serial port. Will retry in a moment");
                        cancellation.WaitHandle.WaitOne(4000);
                    }

                    cancellation.ThrowIfCancellationRequested();
                } while (!m_serialPort.IsOpen);

                do
                {
                    // Wait for data.
                    m_hasDataEvent.Wait(1000, cancellation);
                    m_hasDataEvent.Reset();
                    
                    // Try to send.
                    byte[] sendData = null;
                    lock(m_pendingTransmitData)
                    {
                        if ( m_pendingTransmitData.Count > 0 )
                            sendData = m_pendingTransmitData.Dequeue();
                    }
                    if (sendData != null)
                    {
                        m_serialPort.Write(sendData, 0, sendData.Length);

                        // Assume there's more data - worst case, we looped for nothing.
                        m_hasDataEvent.Set();
                    }

                    // Try to receive. XX bytes at a time.
                    int readBytes = 10;
                    while (readBytes > 0)
                    {
                        var b = SafeReadByte(cancellation);
                        if ( !b.HasValue )
                            break;

                        m_dataReceivedSubject.OnNext(b.Value);
                        --readBytes;
                    }

                    // Read all 10 bytes - there must be more data pending...
                    if (readBytes == 0)
                        m_hasDataEvent.Set();
                } while (true);
            }
            catch (OperationCanceledException)
            {
                // No-op: It's OK to terminate.
            }
            catch (Exception e)
            {
                if (m_cancellationToken.IsCancellationRequested)
                {
                    // No-op: Ignore exceptions when terminating.
                }
                else
                    m_dataReceivedSubject.OnError(e);
            }
        }

        private byte? SafeReadByte(CancellationToken cancellation)
        {
            while (true)
            {
                try
                {
                    int itersLeft = 10;
                    while (m_serialPort.BytesToRead <= 0 && itersLeft > 0)
                    {
                        --itersLeft;
                        cancellation.ThrowIfCancellationRequested();
                        Thread.Sleep(100);
                    }
                    if ( m_serialPort.BytesToRead <= 0 )
                        return null;

                    int b = m_serialPort.ReadByte();
                    if (b == -1)
                        return null;

                    return (byte)b;
                }
                catch (IOException e)
                {
                    // Ignore IO exceptions.
                    Log.Debug(e, "Got IO exception. Ignoring");
                }
            }
        }

        private Thread m_thread;
        private CancellationTokenSource m_cancellationToken;

        #region Implementation of IDisposable

        public void Dispose()
        {
            // Already disposed?
            if (m_cancellationToken == null || m_cancellationToken.IsCancellationRequested)
                return;

            // Mark cancellation.
            m_cancellationToken.Cancel();

            try
            {
                // Try to dispose of the serial port regardless of anything.
                // This will stop any blocking action we might have.
                // NOTE: This must be done before joining the thread. Otherwise, the thread might never terminate.
                m_serialPort.Dispose();
            }
            catch (Exception)
            {
                // Ignore errors.
            }

            // Stop the thread.
            m_thread?.Join();
            m_thread = null;
        }

        #endregion
    }
}