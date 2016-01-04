namespace More.OData.Edm
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="MediaLinkEntryAnnotation"/>.
    /// </summary>
    public class MediaLinkEntryAnnotationTest
    {
        private sealed class Entity
        {
            public string MediaType
            {
                get;
                set;
            }
        }

        [Fact( DisplayName = "get content type should return annotation value" )]
        public void GetContentTypeShouldReturnAnnotationValue()
        {
            // arrange
            var expected = "application/octet-stream";
            var instance = new Entity(){ MediaType = expected };
            var annotation = new MediaLinkEntryAnnotation( o => ( (Entity) o ).MediaType );

            // act
            var actual = annotation.GetContentType( instance );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "get content type with lazy func should return annotation value" )]
        public void GetContentTypeWithLazyFuncShouldReturnAnnotationValue()
        {
            // arrange
            var expected = "application/octet-stream";
            var instance = new Entity() { MediaType = expected };
            var contentType = new Lazy<Func<object, string>>( () => o => ( (Entity) o ).MediaType );
            var annotation = new MediaLinkEntryAnnotation( contentType );

            // act
            var actual = annotation.GetContentType( instance );

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
