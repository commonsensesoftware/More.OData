namespace System.Web.OData.Builder
{
    using Linq;
    using Linq.Expressions;
    using Moq;
    using More.Web.OData.Builder;
    using System;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="AnnotationConfigurationCollection"/>.
    /// </summary>
    public class AnnotationConfigurationCollectionTest
    {
        private sealed class Entity
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [Fact( DisplayName = "add should append the expected items to the collection" )]
        public void AddShouldAppendExpectedItemsToCollection()
        {
            // arrange
            var expected = 3;
            Expression<Func<Entity, int>> property1 = e => e.Id;
            Expression<Func<Entity, string>> property2 = e => e.FirstName;
            Expression<Func<Entity, string>> property3 = e => e.LastName;
            var configuration1 = new Mock<IAnnotationConfiguration>().Object;
            var configuration2 = new Mock<IAnnotationConfiguration>().Object;
            var configuration3 = new Mock<IAnnotationConfiguration>().Object;
            var collection = new AnnotationConfigurationCollection();
            string name;

            // act
            collection.Add( property1.GetInstanceAnnotationKey( out name ), configuration1 );
            collection.Add( property2.GetInstanceAnnotationKey( out name ), configuration2 );
            collection.Add( property3.GetInstanceAnnotationKey( out name ), configuration3 );

            var actual = collection.Count;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "collection should enumerate in expected order" )]
        public void CollectionShouldEnumerateInExpectedOrder()
        {
            // arrange
            Expression<Func<Entity, int>> property1 = e => e.Id;
            Expression<Func<Entity, string>> property2 = e => e.FirstName;
            Expression<Func<Entity, string>> property3 = e => e.LastName;
            var configuration1 = new Mock<IAnnotationConfiguration>().Object;
            var configuration2 = new Mock<IAnnotationConfiguration>().Object;
            var configuration3 = new Mock<IAnnotationConfiguration>().Object;
            var expected = new[] { configuration1, configuration2, configuration3 }.AsEnumerable();
            var collection = new AnnotationConfigurationCollection();
            string name;

            // act
            collection.Add( property1.GetInstanceAnnotationKey( out name ), configuration1 );
            collection.Add( property2.GetInstanceAnnotationKey( out name ), configuration2 );
            collection.Add( property3.GetInstanceAnnotationKey( out name ), configuration3 );

            var actual = collection.AsEnumerable();

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "indexer should return item at expected index" )]
        public void IndexerShouldReturnItemAtExpectedIndex()
        {
            // arrange
            Expression<Func<Entity, int>> property1 = e => e.Id;
            Expression<Func<Entity, string>> property2 = e => e.FirstName;
            Expression<Func<Entity, string>> property3 = e => e.LastName;
            var configuration1 = new Mock<IAnnotationConfiguration>().Object;
            var configuration2 = new Mock<IAnnotationConfiguration>().Object;
            var configuration3 = new Mock<IAnnotationConfiguration>().Object;
            var expected = new[] { configuration1, configuration2, configuration3 };
            var collection = new AnnotationConfigurationCollection();
            string name;

            // act
            collection.Add( property1.GetInstanceAnnotationKey( out name ), configuration1 );
            collection.Add( property2.GetInstanceAnnotationKey( out name ), configuration2 );
            collection.Add( property3.GetInstanceAnnotationKey( out name ), configuration3 );

            // assert
            for ( var i = 0; i < expected.Length; i++ )
                Assert.Same( expected[i], collection[i] );
        }

        [Fact( DisplayName = "add should not allow the same property more than once" )]
        public void AddShouldNotAllowSamePropertyMoreThanOnce()
        {
            // arrange
            var expected = $"The property '{typeof( Entity ).FullName}.Id' cannot be configured as an annotation more than once.";
            Expression<Func<Entity, int>> property = e => e.Id;
            var configuration = new Mock<IAnnotationConfiguration>().Object;
            var collection = new AnnotationConfigurationCollection();

            collection.Add( property.GetMediaResourceKey(), configuration );

            // act
            var ex = Assert.Throws<InvalidOperationException>( () => collection.Add( property.GetMediaResourceKey(), configuration ) );
            var actual = ex.Message;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "try get should return false when item is not in collection" )]
        public void TryGetShouldReturnFalseWhenItemIsNotInCollection()
        {
            // arrange
            Expression<Func<Entity, int>> property = e => e.Id;
            IAnnotationConfiguration item;
            var collection = new AnnotationConfigurationCollection();

            // act
            var result = collection.TryGet( property.GetMediaResourceKey(), out item );

            // assert
            Assert.False( result );
            Assert.Null( item );
        }

        [Fact( DisplayName = "try get should return true when item is in collection" )]
        public void TryGetShouldReturnTrueWhenItemIsInCollection()
        {
            // arrange
            Expression<Func<Entity, int>> property = e => e.Id;
            var configuration = new Mock<IAnnotationConfiguration>().Object;
            IAnnotationConfiguration item;
            var collection = new AnnotationConfigurationCollection();

            collection.Add( property.GetMediaResourceKey(), configuration );

            // act
            var result = collection.TryGet( property.GetMediaResourceKey(), out item );

            // assert
            Assert.True( result );
            Assert.Same( configuration, item );
        }
    }
}
