namespace More.Web.OData
{
    using System;
    using System.IO;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="EmptyStream"/>.
    /// </summary>
    public class EmptyStreamTest
    {
        [Fact( DisplayName = "new empty stream should set expected length" )]
        public void ConstructorShouldSetExpectedLength()
        {
            // arrange
            var length = 100L;

            // act
            var stream = new EmptyStream( length );

            // assert
            Assert.Equal( length, stream.Length );
        }

        [Fact( DisplayName = "can read should be true" )]
        public void CanReadShouldBeTrue()
        {
            // arrange
            var expected = true;
            var stream = new EmptyStream( 0L );

            // act
            var actual = stream.CanRead;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "can seek should be true" )]
        public void CanSeekShouldBeTrue()
        {
            // arrange
            var expected = true;
            var stream = new EmptyStream( 0L );

            // act
            var actual = stream.CanSeek;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "can write should be false" )]
        public void CanWriteShouldBeFalse()
        {
            // arrange
            var expected = false;
            var stream = new EmptyStream( 0L );

            // act
            var actual = stream.CanWrite;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "position should be updateable within range" )]
        public void PositionShouldBeUpdateableWithinRange()
        {
            // arrange
            var expected = 50L;
            var stream = new EmptyStream( 100L );

            // act
            stream.Position = expected;
            var actual = stream.Position;

            // assert
            Assert.Equal( expected, actual );
        }

        [Theory( DisplayName = "position should not allow value out of range" )]
        [InlineData( 10, 15 )]
        [InlineData( 0, 10 )]
        public void PositionShouldNotAllowValueOutOfRange( long length, long position )
        {
            // arrange
            var stream = new EmptyStream( length );

            // act
            var ex = Assert.Throws<ArgumentOutOfRangeException>( () => stream.Position = position );

            // assert
            Assert.Equal( "value", ex.ParamName );
        }

        [Fact( DisplayName = "set length should not be allowed" )]
        public void SetLengthShouldNotBeAllowed()
        {
            // arrange
            var stream = new EmptyStream( 0L );

            // act
            var ex = Assert.Throws<NotSupportedException>( () => stream.SetLength( 100L ) );

            // assert

        }

        [Fact( DisplayName = "write should not be allowed" )]
        public void WriteShouldNotBeAllowed()
        {
            // arrange
            var buffer = new byte[100];
            var stream = new EmptyStream( 0L );

            // act
            var ex = Assert.Throws<NotSupportedException>( () => stream.Write( buffer, 0, buffer.Length ) );

            // assert

        }

        [Theory( DisplayName = "read should consume stream" )]
        [InlineData( 20, 45L )]
        [InlineData( 30, 60L )]
        public void ReadShouldConsumeStream( int size, long length )
        {
            // arrange
            var expected = length - 1;
            var buffer = new byte[size];
            var stream = new EmptyStream( length );
            var count = 0;

            // act
            while ( ( count = stream.Read( buffer, 0, size ) ) > 0 )
                ;

            var actual = stream.Position;

            // assert
            Assert.Equal( expected, actual );
        }

        [Theory( DisplayName = "seek should move to expected position" )]
        [InlineData( 100L, 0L, SeekOrigin.Begin, 0L )]
        [InlineData( 100L, 50L, SeekOrigin.Current, 50L )]
        [InlineData( 100L, 10L, SeekOrigin.End, 90L )]
        public void SeekShouldMoveToExpectedPosition( long length, long offset, SeekOrigin origin, long expected )
        {
            // arrange
            var stream = new EmptyStream( length );

            // act
            var actual = stream.Seek( offset, origin );

            // assert
            Assert.Equal( expected, actual );
        }

        [Theory( DisplayName = "seek should not allow offset out of range" )]
        [InlineData( 100L, 100L, SeekOrigin.Begin )]
        [InlineData( 100L, 100L, SeekOrigin.Current )]
        [InlineData( 100L, 101L, SeekOrigin.End )]
        public void SeekShouldNotAllowOffsetOutOfRange( long length, long offset, SeekOrigin origin )
        {
            // arrange
            var stream = new EmptyStream( length );

            // act
            var ex = Assert.Throws<ArgumentOutOfRangeException>( () => stream.Seek( offset, origin ) );

            // assert
            Assert.Equal( nameof( offset ), ex.ParamName );
        }
    }
}
