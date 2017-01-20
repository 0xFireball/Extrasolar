using System;
using System.IO;
using System.Threading;

namespace Extrasolar.Tests.Mocks
{
    /// <summary>
    /// http://stackoverflow.com/a/1484960/6591463
    /// </summary>
    public class UnlimitedMemoryStream : MemoryStream
    {
        private AutoResetEvent _dataReadyEvent = new AutoResetEvent(false);
        private byte[] m_buffer;
        private int m_offset;
        private int m_count;

        public override void Write(byte[] buffer, int offset, int count)
        {
            m_buffer = buffer;
            m_offset = offset;
            m_count = count;
            _dataReadyEvent.Set();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_buffer == null)
            {
                // Block until the stream has some more data.
                _dataReadyEvent.WaitOne();
            }

            Buffer.BlockCopy(m_buffer, 0, buffer, offset, m_count);
            m_buffer = null;
            return (count < m_count) ? count : m_count;
        }
    }
}