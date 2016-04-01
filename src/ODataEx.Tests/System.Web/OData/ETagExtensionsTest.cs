namespace System.Web.OData
{
    using Collections.Generic;
    using Formatter;
    using System;
    using System.Dynamic;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="ETagExtensions"/>.
    /// </summary>
    public class ETagExtensionsTest
    {
        private sealed class TestSetMemberBinder : SetMemberBinder
        {
            internal TestSetMemberBinder( string memberName )
                : base( memberName, false )
            {
            }

            public override DynamicMetaObject FallbackSetMember( DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion )
            {
                throw new NotSupportedException();
            }
        }

        private ETag etag = new ETag();

        public ETagExtensionsTest()
        {
            etag.TrySetMember( new TestSetMemberBinder( "id" ), 42 );
            etag.TrySetMember( new TestSetMemberBinder( "version" ), BitConverter.GetBytes( 42L ) );
        }

        [Fact( DisplayName = "get value should return expected value" )]
        public void GetValueShouldReturnExpectedValue()
        {
            // arrange
            var expected = 42;

            // act
            var actual = etag.GetValue<int>( "id" );

            // assert
            Assert.Equal( expected, actual );
        }

        [Theory( DisplayName = "has should match expected value" )]
        [InlineData( 42, true )]
        [InlineData( 10, false )]
        public void HasShouldMatchExpectedValue( int value, bool expected )
        {
            // arrange

            // act
            var actual = etag.Has( "id", value );

            // assert
            Assert.Equal( expected, actual );
        }

        [Theory( DisplayName = "has should match expected row version" )]
        [InlineData( 42L, true )]
        [InlineData( 100L, false )]
        public void HasShouldMatchExpectedRowVersion( long value, bool expected )
        {
            // arrange
            var rowVersion = BitConverter.GetBytes( value );

            // act
            var actual = etag.HasRowVersion( "version", rowVersion );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "has all should return true when all values are matched" )]
        public void HasAllShouldReturnTrueWhenAllValuesAreMatched()
        {
            // arrange
            var expected = true;
            var values = new Dictionary<string, object>()
            {
                { "id", 42 },
                { "version", BitConverter.GetBytes( 42L ) }
            };

            // act
            var actual = etag.HasAll( values );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "has all should return false when all values are not matched" )]
        public void HasAllShouldReturnFalseWhenAllValuesAreNotMatched()
        {
            // arrange
            var expected = false;
            var values = new Dictionary<string, object>()
            {
                { "id", 42 },
                { "version", BitConverter.GetBytes( 42L ) },
                { "salt", "12345" }
            };

            // act
            var actual = etag.HasAll( values );

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
