using System;
using System.Text;

namespace Xperitos.Common.Streams
{
    class ProducerConsumerBuffer
    {
        public ProducerConsumerBuffer(int maxCapacity = 5 * 1024 * 1024)
        {
            m_maxCapacity = maxCapacity;
            m_buffer = new byte[1024];
        }

        private readonly int m_maxCapacity;

        private byte[] m_buffer;
        private int m_writePtr;
        private int m_readPtr;

        public int UsedCapacity { get { return m_writePtr - m_readPtr; } }

        public void GetWritePtr(int maxSize, out byte[] buffer, out int offset)
        {
            if ( maxSize > m_maxCapacity - UsedCapacity )
                throw new OutOfMemoryException("read size exceeds max capacity");

            // Increase the buffer?
            if (m_buffer.Length - m_writePtr < maxSize)
            {
                int used = UsedCapacity;

                // Check if moving the write ptr back is enough.
                if (maxSize <= m_buffer.Length - UsedCapacity)
                    Buffer.BlockCopy(m_buffer, m_readPtr, m_buffer, 0, used);
                else
                {
                    // Reallocate the buffer.
                    int newBufferSize = m_buffer.Length * 2;
                    if ( newBufferSize < maxSize )
                        newBufferSize = maxSize;
                    var newBuffer = new byte[newBufferSize];

                    if (used > 0)
                        Buffer.BlockCopy(m_buffer, m_readPtr, newBuffer, 0, used);
                    m_buffer = newBuffer;
                }

                m_readPtr = 0;
                m_writePtr = used;
            }

            buffer = m_buffer;
            offset = m_writePtr;
        }

        public int GetReadPtr(out byte[] buffer, out int offset)
        {
            buffer = m_buffer;
            offset = m_readPtr;

            return UsedCapacity;
        }

        /// <summary>
        /// Extracts the entire buffer and clears this instance.
        /// </summary>
        /// <returns></returns>
        public byte[] ExtractReadBuffer()
        {
            if ( UsedCapacity == 0 )
                return new byte[0];

            var result = new byte[UsedCapacity];
            Buffer.BlockCopy(m_buffer, m_readPtr, result, 0, UsedCapacity);
            m_readPtr = 0;
            m_writePtr = 0;

            return result;
        }

        /// <summary>
        /// Clear the buffer.
        /// </summary>
        public void Clear()
        {
            m_readPtr = m_writePtr = 0;
        }

        public void AddByte(byte value)
        {
            byte[] buffer;
            int offset;
            GetWritePtr(1, out buffer, out offset);
            buffer[offset] = value;
            Produced(1);
        }

        public void AddBytes(byte[] value)
        {
            byte[] buffer;
            int offset;
            GetWritePtr(value.Length, out buffer, out offset);
            Buffer.BlockCopy(value, 0, buffer, offset, value.Length);
            Produced(value.Length);
        }

        public byte ConsumeByte()
        {
            if ( UsedCapacity == 0 )
                throw new InvalidOperationException("Nothing to consume!");

            byte result = m_buffer[m_readPtr];
            Consumed(1);

            return result;
        }

        public byte[] ConsumeBytes(int bytes)
        {
            if (bytes > UsedCapacity)
                throw new InvalidOperationException("Not enough data to consume!");

            var result = new byte[bytes];
            Buffer.BlockCopy(m_buffer, m_readPtr, result, 0, bytes);
            Consumed(bytes);
            return result;
        }

        public void Produced(int bytes)
        {
            m_writePtr += bytes;
        }

        public void Consumed(int bytes)
        {
            if (bytes > UsedCapacity)
                throw new InvalidOperationException("Not enough data to consume!");

            m_readPtr += bytes;
            if ( m_readPtr == m_writePtr )
                m_readPtr = m_writePtr = 0;
        }
    }

    static class ProducerConsumerBufferMixins
    {
        /// <summary>
        /// Returns the entire <see cref="ProducerConsumerBuffer"/> buffer as string. Does not consume.
        /// </summary>
        public static string PeekString(this ProducerConsumerBuffer pcb, Encoding encoding = null)
        {
            if ( encoding == null )
                encoding = Encoding.ASCII;

            byte[] buffer;
            int offset;
            int count = pcb.GetReadPtr(out buffer, out offset);
            if ( count == 0 )
                return "";

            return encoding.GetString(buffer, offset, count);
        }

        public static string ExtractAsString(this ProducerConsumerBuffer pcb, Encoding encoding = null)
        {
            var result = pcb.PeekString(encoding);
            pcb.Clear();
            return result;
        }
    }
}
