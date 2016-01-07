namespace More.OData.Edm
{
    using Moq;
    using System;
    using System.Web.OData.Builder;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="InstanceAnnotation"/>.
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

        [Fact( DisplayName = "new entity instance annotation should set expected qualified name" )]
        public void ConstructorWithFuncShouldSetExpectedQualifiedName()
        {
            // arrange
            var expected = "Testing.Test";
            var annotation = new InstanceAnnotation( o => o, expected, new Mock<IEdmTypeConfiguration>().Object );

            // act
            var actual = annotation.QualifiedName;

            // assert
            Assert.Equal( expected, annotation.QualifiedName );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected annotation type name" )]
        public void ConstructorWithFuncShouldSetExpectedAnnotationName()
        {
            // arrange
            var expected = "Test";
            var typeConfig = new Mock<IEdmTypeConfiguration>();

            typeConfig.SetupGet( tc => tc.FullName ).Returns( expected );

            var annotation = new InstanceAnnotation( o => o, "testing.annotation", typeConfig.Object );

            // act
            var actual = annotation.AnnotationTypeName;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected qualified name" )]
        public void ConstructorWithLazyFuncShouldSetExpectedName()
        {
            // arrange
            var expected = "Testing.Test";
            var annotation = new InstanceAnnotation( new Lazy<Func<object, object>>( () => o => o ), expected, new Mock<IEdmTypeConfiguration>().Object );

            // act
            var actual = annotation.QualifiedName;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "new entity instance annotation should set expected annotation type name" )]
        public void ConstructorWithLazyFuncShouldSetExpectedAnnotationTypeName()
        {
            // arrange
            var expected = "Test";
            var typeConfig = new Mock<IEdmTypeConfiguration>();

            typeConfig.SetupGet( tc => tc.FullName ).Returns( expected );

            var annotation = new InstanceAnnotation( new Lazy<Func<object, object>>( () => o => o ), "testing.annotation", typeConfig.Object );

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
            var annotation = new InstanceAnnotation( o => ( (Entity) o ).Timestamp, "testing.timestamp", new Mock<IEdmTypeConfiguration>().Object );

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
            var annotation = new InstanceAnnotation( accessor, "testing.timestamp", new Mock<IEdmTypeConfiguration>().Object );

            // act
            var actual = annotation.GetValue( instance );

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
