using System;
using System.IO;
using System.Threading;

namespace Extrasolar.Tests.Mocks
{
    public class UnlimitedMemoryStream : Stream
    {
        private MemoryStream _memStrm = new MemoryStream();
        private readonly AutoResetEvent _dataReadyWaitHandle = new AutoResetEvent(false);
        private int _timeout = 5000;

        private long readPosition;
        private long writePosition;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _memStrm.Length;

        public override long Position
        {
            get
            {
                return _memStrm.Position;
            }

            set
            {
                _memStrm.Position = value;
            }
        }

        public override void Flush()
        {
            _memStrm.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //return _memStrm.Read(buffer, offset, count);
            int read;
            _memStrm.Position = readPosition;
            while ((read = _memStrm.Read(buffer, offset, count)) == 0)
            {
                // No data, wait for data
                _dataReadyWaitHandle.WaitOne(_timeout);
            }
            readPosition = _memStrm.Position;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            // Write and notify
            _memStrm.Position = writePosition;
            _memStrm.Write(buffer, offset, count);
            writePosition = _memStrm.Position;
            _dataReadyWaitHandle.Set();
        }
    }
}