namespace More.OData.Edm
{
    using System;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="EntityInstanceAnnotation"/>.
    /// </summary>
    public class EntityInstanceAnnotationTest
    {
        private sealed class Entity
        {
            public TimeSpan Timestamp
            {
                get;
                set;
            }
        }

        [Fact( DisplayName = "new entity instance annotation should set expected name" )]
        public void ConstructorWithFuncShouldSetExpectedName()
        {
            // arrange
            var name = "Test";
            var qualifiedName = "Testing.Test";

            // act
            var annotation = new EntityInstanceAnnotation( o => o, "Testing", name, "annotationType" );

            // assert
            Assert.Equal( name, annotation.Name );
            Assert.Equal( qualifiedName, annotation.QualifiedName );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected annotation type name" )]
        public void ConstructorWithFuncShouldSetExpectedAnnotationName()
        {
            // arrange
            var expected = "Test";
            var annotation = new EntityInstanceAnnotation( o => o, "testing", "annotation", expected );

            // act
            var actual = annotation.AnnotationTypeName;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected name" )]
        public void ConstructorWithLazyFuncShouldSetExpectedName()
        {
            // arrange
            var name = "Test";
            var qualifiedName = "Testing.Test";

            // act
            var annotation = new EntityInstanceAnnotation( new Lazy<Func<object, object>>( () => o => o ), "Testing", name, "annotationType" );

            // assert
            Assert.Equal( name, annotation.Name );
            Assert.Equal( qualifiedName, annotation.QualifiedName );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected annotation type name" )]
        public void ConstructorWithLazyFuncShouldSetExpectedAnnotationTypeName()
        {
            // arrange
            var expected = "Test";
            var annotation = new EntityInstanceAnnotation( new Lazy<Func<object, object>>( () => o => o ), "testing", "annotation", expected );

            // act
            var actual = annotation.AnnotationTypeName;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "get value should return annotation value" )]
        public void GetValueShouldReturnAnnotationValue()
        {
            // arrange
            var expected = new TimeSpan( 8, 0, 0 );
            var instance = new Entity() { Timestamp = expected };
            var annotation = new EntityInstanceAnnotation( o => ( (Entity) o ).Timestamp, "testing", "timestamp", "Test.Timestamp" );

            // act
            var actual = annotation.GetValue( instance );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "get value with lazy func should return annotation value" )]
        public void GetValueWithLazyFuncShouldReturnAnnotationValue()
        {
            // arrange
            var expected = new TimeSpan( 8, 0, 0 );
            var instance = new Entity() { Timestamp = expected };
            var accessor = new Lazy<Func<object, object>>( () => o => ( (Entity) o ).Timestamp );
            var annotation = new EntityInstanceAnnotation( accessor, "testing", "timestamp", "Test.Timestamp" );

            // act
            var actual = annotation.GetValue( instance );

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
