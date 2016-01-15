namespace System.Web.OData.Builder
{
    using Linq.Expressions;
    using System;
    using System.Collections.Generic;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="PropertyPathVisitor"/>.
    /// </summary>
    public class PropertyPathVisitorTest
    {
        private sealed class Entity
        {
            public IList<object> Items => new List<object>();
            public IDictionary<string, object> History => new Dictionary<string, object>();
            public object AuditInfo( string key ) => null;
        }

        [Fact( DisplayName = "visitor should resolve property name" )]
        public void VisitorShouldResolvePropertyName()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.Items;
            var visitor = new PropertyPathVisitor();

            // act
            visitor.Visit( expression );

            // assert
            Assert.Equal( nameof( Entity.Items ), visitor.Name );
        }

        [Fact( DisplayName = "visitor should resolve property name from path" )]
        public void VisitorShouldResolvePropertyNameFromPath()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.Items.Count;
            var visitor = new PropertyPathVisitor();

            // act
            visitor.Visit( expression );

            // assert
            Assert.Equal( nameof( IList<object>.Count ), visitor.Name );
        }

        [Fact( DisplayName = "visitor should resolve property name with indexer" )]
        public void VisitorShouldResolvePropertyNameWithIndexer()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.Items[0];
            var visitor = new PropertyPathVisitor();

            // act
            visitor.Visit( expression );

            // assert
            Assert.Equal( nameof( Entity.Items ), visitor.Name );
        }

        [Fact( DisplayName = "visitor should resolve method name" )]
        public void VisitorShouldResolveMethodName()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.AuditInfo( "test" );
            var visitor = new PropertyPathVisitor();

            // act
            visitor.Visit( expression );

            // assert
            Assert.Equal( nameof( Entity.AuditInfo ), visitor.Name );
        }

        [Fact( DisplayName = "visitor should generate key" )]
        public void VisitorShouldGenerateKey()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.Items;
            var visitor = new PropertyPathVisitor();
            var expected = $"{typeof( Entity ).FullName}.{nameof( Entity.Items )}";

            // act
            visitor.Visit( expression );
            var actual = visitor.GenerateKey( typeof( Entity ).FullName );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "visitor should generate key with indexer" )]
        public void VisitorShouldGenerateKeyWithIndexer()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.Items[0];
            var visitor = new PropertyPathVisitor();
            var expected = $"{typeof( Entity ).FullName}.{nameof( Entity.Items )}_0";

            // act
            visitor.Visit( expression );
            var actual = visitor.GenerateKey( typeof( Entity ).FullName );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "visitor should generate key with key" )]
        public void VisitorShouldGenerateKeyWithKey()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.History["test"];
            var visitor = new PropertyPathVisitor();
            var expected = $"{typeof( Entity ).FullName}.{nameof( Entity.History )}_test";

            // act
            visitor.Visit( expression );
            var actual = visitor.GenerateKey( typeof( Entity ).FullName );

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "visitor should generate key with parameter" )]
        public void VisitorShouldGenerateKeyWithParameter()
        {
            // arrange
            Expression<Func<Entity, object>> expression = o => o.AuditInfo( "test" );
            var visitor = new PropertyPathVisitor();
            var expected = $"{typeof( Entity ).FullName}.{nameof( Entity.AuditInfo )}_test";

            // act
            visitor.Visit( expression );
            var actual = visitor.GenerateKey( typeof( Entity ).FullName );

            // assert
            Assert.Equal( expected, actual );
        }
    }
}
