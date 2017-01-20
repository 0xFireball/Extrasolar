using System;
using System.IO;

namespace Extrasolar.Tests.Mocks
{
    public class UnlimitedMemoryStream : Stream
    {
        private MemoryStream _memStrm = new MemoryStream();

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
            return _memStrm.Read(buffer, offset, count);
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
            _memStrm.Write(buffer, offset, count);
        }
    }
}