using System;
using System.IO;
using System.Text;

namespace GetText.Loaders
{
    /// <summary>
    /// Reads primitive data types as binary values represented in big endian byte order.
    /// </summary>
    public class BigEndianBinaryReader : BinaryReader
    {
        private readonly byte[] buffer = new byte[16];

        /// <summary>
        /// Initializes a new instance of the <see cref="BigEndianBinaryReader"/> class based on the 
        /// supplied stream and using System.Text.UTF8Encoding.
        /// </summary>
        /// <param name="input"></param>
        public BigEndianBinaryReader(Stream input) : base(input)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigEndianBinaryReader"/> class based on the
        /// supplied stream and a specific character encoding.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding"></param>
        public BigEndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BigEndianBinaryReader"/> class based on the
        /// supplied stream and a specific character encoding.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="encoding"></param>
        /// <param name="leaveOpen"></param>
        public BigEndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current 
        /// stream and advances the current position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override short ReadInt16()
        {
            FillBuffer(2);
            return (short)(buffer[1] | buffer[0] << 8);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current 
        /// stream using little-endian encoding and advances the position of the stream by two bytes.
        /// </summary>
        /// <returns>
        /// A 2-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [CLSCompliant(false)]
        public override ushort ReadUInt16()
        {
            FillBuffer(2);
            return (ushort)(buffer[1] | buffer[0] << 8);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current 
        /// stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override int ReadInt32()
        {
            FillBuffer(4);
            return buffer[3] | buffer[2] << 8 | buffer[1] << 16 | buffer[0] << 24;
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current 
        /// stream and advances the position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        [CLSCompliant(false)]
        public override uint ReadUInt32()
        {
            FillBuffer(4);
            return (uint)(buffer[3] | buffer[2] << 8 | buffer[1] << 16 | buffer[0] << 24);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current 
        /// stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte signed integer read from the current stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override long ReadInt64()
        {
            FillBuffer(8);
            if (BitConverter.IsLittleEndian)
            {
                // We don't need to reverse bytes on big-endian machine as BitConverter respects machine endianness
                Array.Reverse(buffer, 0, 8);
            }
            return BitConverter.ToInt64(buffer, 0);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current 
        /// stream and advances the position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte unsigned integer read from this stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <filterpriority>2</filterpriority>
        [CLSCompliant(false)]
        public override ulong ReadUInt64()
        {
            FillBuffer(8);
            if (BitConverter.IsLittleEndian)
            {
                // We don't need to reverse bytes on big-endian machine as BitConverter respects machine endianness
                Array.Reverse(buffer, 0, 8);
            }
            return BitConverter.ToUInt64(buffer, 0);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current 
        /// stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>
        /// A 4-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override float ReadSingle()
        {
            FillBuffer(4);
            if (BitConverter.IsLittleEndian)
            {
                // We don't need to reverse bytes on big-endian machine as BitConverter respects machine endianness
                Array.Reverse(buffer, 0, 4);
            }
            return BitConverter.ToSingle(buffer, 0);
        }

        /// <summary>
        /// Reads an 8-byte floating point value from the current 
        /// stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>
        /// An 8-byte floating point value read from the current stream.
        /// </returns>
        /// <exception cref="EndOfStreamException">The end of the stream is reached. </exception>
        /// <exception cref="ObjectDisposedException">The stream is closed. </exception>
        /// <exception cref="IOException">An I/O error occurs. </exception>
        /// <filterpriority>2</filterpriority>
        public override double ReadDouble()
        {
            FillBuffer(8);
            if (BitConverter.IsLittleEndian)
            {
                // We don't need to reverse bytes on big-endian machine as BitConverter respects machine endianness
                Array.Reverse(buffer, 0, 8);
            }
            return BitConverter.ToDouble(buffer, 0);
        }


        protected override void FillBuffer(int numBytes)
        {
            if (numBytes < 2 || numBytes > buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(numBytes));
            }
            int bytesRead = 0;
            Stream stream = BaseStream;
            if (stream == null)
                throw new ObjectDisposedException("Base stream closed.");

            do
            {
                int n = stream.Read(buffer, bytesRead, numBytes - bytesRead);
                if (n == 0)
                {
                    throw new EndOfStreamException("Unexpected End Of File.");
                }
                bytesRead += n;
            } while (bytesRead < numBytes);
        }
    }
}
