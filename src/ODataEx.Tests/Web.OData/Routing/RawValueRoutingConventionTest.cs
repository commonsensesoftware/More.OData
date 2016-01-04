namespace More.Web.OData.Routing
{
    using Moq;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;
    using System.Web.OData.Routing;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="RawValueRoutingConvention"/>.
    /// </summary>
    public class RawValueRoutingConventionTest
    {
        [Theory( DisplayName = "select action should return expected action name" )]
        [InlineData( "GET", true, "GetValue" )]
        [InlineData( "GET", false, null )]
        [InlineData( "POST", true, "PostValue" )]
        [InlineData( "POST", false, null )]
        [InlineData( "PUT", true, "PutValue" )]
        [InlineData( "PUT", false, null )]
        [InlineData( "DELETE", true, "DeleteValue" )]
        [InlineData( "DELETE", false, null )]
        [InlineData( "PATCH", true, "PatchValue" )]
        [InlineData( "PATCH", false, null )]
        public void SelectActionShouldReturnExpectedActionName( string method, bool contains, string expected )
        {
            // arrange
            var odataPath = new ODataPath( new EntitySetPathSegment( "Tests" ), new KeyValuePathSegment( "1" ), new ValuePathSegment() );
            var request = new HttpRequestMessage( new HttpMethod( method ), "http://localhost/Tests(1)/$value" );
            var configuration = new HttpConfiguration();
            var values = new HttpRouteValueDictionary() { { "entityset", "Tests" }, { "key", null } };
            var routeData = new HttpRouteData( new HttpRoute(), values );
            var controllerContext = new HttpControllerContext( configuration, routeData, request );
            var actionMap = new Mock<ILookup<string, HttpActionDescriptor>>();
            var convention = new RawValueRoutingConvention();

            actionMap.Setup( am => am.Contains( expected ) ).Returns( contains );

            // act
            var actual = convention.SelectAction( odataPath, controllerContext, actionMap.Object );

            // assert
            Assert.Equal( expected, actual );
            Assert.Equal( "1", values["key"] );
        }
    }
}
