namespace More.Web.OData
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.IO;

    /// <summary>
    /// Represents an empty stream which is capable of reporting its content length.
    /// </summary>
    /// <remarks>This <see cref="Stream"/> can be used to optimize scenarios which require content,
    /// but do not return any content such as the response to the HTTP HEAD verb.</remarks>
    public class EmptyStream : Stream
    {
        private readonly long length;
        private long position;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyStream"/> class.
        /// </summary>
        /// <param name="length">The length of the stream.</param>
        public EmptyStream( long length )
        {
            Arg.InRange( length, 0L, nameof( length ) );
            this.length = length;
        }

        /// <summary>
        /// Gets a value indicating whether the stream can be read.
        /// </summary>
        /// <value>True if the stream can be read; otherwise, false.</value>
        /// <remarks>This property always returns <c>true</c>.</remarks>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream supports seeking.
        /// </summary>
        /// <value>True if the stream supports seeking; otherwise, false.</value>
        /// <remarks>This property always returns <c>true</c>.</remarks>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream can be written to.
        /// </summary>
        /// <value>True if the stream can be written to; otherwise, false.</value>
        /// <remarks>This property always returns <c>false</c>.</remarks>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the length of the stream.
        /// </summary>
        /// <value>The length or size of the stream.</value>
        public override long Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the stream.
        /// </summary>
        /// <value>The position within the stream.</value>
        public override long Position
        {
            get
            {
                Contract.Ensures( position >= 0L );
                return position;
            }
            set
            {
                Arg.InRange( value, 0L, Length - 1L, nameof( value ) );
                position = value;
            }
        }

        /// <summary>
        /// Flushes any pending write operations.
        /// </summary>
        /// <remarks>This method performs no action.</remarks>
        public override void Flush()
        {
        }

        /// <summary>
        /// Reads bytes from the stream into the specified buffer and advances the position in the stream.
        /// </summary>
        /// <param name="buffer">The buffer to read stream content into.</param>
        /// <param name="offset">The offset in the <paramref name="buffer"/> to read bytes into.</param>
        /// <param name="count">The maximum number of bytes to read from the steam.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested
        /// if that many bytes are not currently available, or zero if the end of the stream has been reached.</returns>
        /// <remarks>This method simulates a read operation and advances the stream position accordingly. No operations read
        /// or copy operations are performed on the <paramref name="buffer"/>.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Unnecessary; the buffer isn't used." )]
        public override int Read( byte[] buffer, int offset, int count )
        {
            // simulate a read, but merely calculate where the stream would advance to
            var end = Length - 1L;
            var pos = Position;

            if ( pos == end )
                return 0;

            var max = (long) count;
            var remaining = Math.Max( 0L, end - pos );
            var read = Math.Min( remaining, max );

            Position = Math.Min( end, pos + max );

            return (int) read;
        }

        /// <summary>
        /// Seeks a position in the stream based on the specified offet and origin.
        /// </summary>
        /// <param name="offset">The offset from the origin.</param>
        /// <param name="origin">One of the <see cref="SeekOrigin"/> values.</param>
        /// <returns>The new position.</returns>
        public override long Seek( long offset, SeekOrigin origin )
        {   
            var newPosition = 0L;
            var size = Length;

            switch ( origin )
            {
                case SeekOrigin.Begin:
                    newPosition = offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = Position + offset;
                    break;
                case SeekOrigin.End:
                    newPosition = size - offset;
                    break;
            }

            // ensure the seek operation didn't take us out of bounds
            if ( newPosition < 0L || newPosition >= size )
                throw new ArgumentOutOfRangeException( nameof( offset ), ExceptionMessage.SeekOffsetOutOfRange );

            return Position = newPosition;
        }

        /// <summary>
        /// Sets the length of the stream.
        /// </summary>
        /// <param name="value">The new length of the stream.</param>
        /// <remarks>This method always throws <see cref="NotSupportedException"/>.</remarks>
        /// <exception cref="NotSupportedException">Occurs from any use of this method.</exception>
        public override void SetLength( long value )
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes the specified buffer into the stream.
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset within the buffer to write from.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <remarks>This method always throws <see cref="NotSupportedException"/>.</remarks>
        /// <exception cref="NotSupportedException">Occurs from any use of this method.</exception>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Unnecessary; the stream is read-only." )]
        public override void Write( byte[] buffer, int offset, int count )
        {
            throw new NotSupportedException();
        }
    }
}
