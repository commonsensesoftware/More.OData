namespace Microsoft.OData.Edm
{
    using Expressions;
    using Moq;
    using System.Linq;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="EdmExtensions"/>.
    /// </summary>
    public class EdmExtensionsTest
    {
        [Fact( DisplayName = "clone should copy action with qualified name" )]
        public void CloneWithQualifiedNameShouldReturnExpectedResult()
        {
            // arrange
            var parameter = new Mock<IEdmOperationParameter>();
            var action = new Mock<IEdmAction>();

            parameter.SetupGet( p => p.Name ).Returns( "obj" );
            parameter.SetupGet( p => p.Type ).Returns( new Mock<IEdmTypeReference>().Object );
            parameter.SetupGet( p => p.DeclaringOperation ).Returns( () => action.Object );
            action.SetupGet( a => a.Namespace ).Returns( "Testing" );
            action.SetupGet( a => a.Name ).Returns( "TestAction" );
            action.SetupGet( a => a.IsBound ).Returns( true );
            action.SetupGet( a => a.ReturnType ).Returns( new Mock<IEdmTypeReference>().Object );
            action.SetupGet( a => a.EntitySetPath ).Returns( new Mock<IEdmPathExpression>().Object );
            action.SetupGet( a => a.Parameters ).Returns( () => new[] { parameter.Object } );

            // act
            var clone = action.Object.CloneWithQualifiedName();

            // assert
            Assert.Equal( "Testing", clone.Namespace );
            Assert.Equal( "Testing.TestAction", clone.Name );
            Assert.True( action.Object.Parameters.SequenceEqual( clone.Parameters ) );
        }
    }
}
