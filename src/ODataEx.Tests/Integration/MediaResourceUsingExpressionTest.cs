namespace More.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;
    using Xunit;
    using static System.Net.HttpStatusCode;

    /// <summary>
    /// Provides integration unit tests for OData media resources using a property expression.
    /// </summary>
    public class MediaResourceUsingExpressionTest : WebApiUnitTest
    {
        protected override void Initialize( HttpConfiguration configuration )
        {
            var builder = new ODataConventionModelBuilder();

            builder.Namespace = "Test";
            builder.EnableLowerCamelCase();
            configuration.EnableCaseInsensitive( true );
            configuration.EnableMediaResources();

            var people = builder.EntitySet<Person>( "People" );
            var person = people.EntityType;

            person.Namespace = builder.Namespace;
            person.HasKey( p => p.Id );
            person.MediaType( p => p.PhotoImageType );
            person.Ignore( p => p.Flags );
            person.Ignore( p => p.Timestamp );
            person.Ignore( p => p.SeoTerms );
            person.Ignore( p => p.Links );
            person.Ignore( p => p.PhotoImage );
            person.Ignore( p => p.Birthday );
            person.Ignore( p => p.DisplayStyle );

            var model = builder.GetEdmModelWithAnnotations();

            configuration.MapODataServiceRoute( "odata", "api", model );
        }

        [Fact( DisplayName = "http get should return entity with media resource link" )]
        public async Task HttpGetShouldReturnEntityWithMediaResourceLink()
        {
            // arrange
            var request = NewGetRequest( "api/people(1)" );

            // act
            var response = await SendAsync( request );
            var json = await response.Content.ReadAsAsync<IDictionary<string, object>>();
            var mediaReadLink = (string) json["@odata.mediaReadLink"];
            var mediaContentType = (string) json["@odata.mediaContentType"];

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( "http://localhost/api/People(1)/$value", mediaReadLink );
            Assert.Equal( "image/png", mediaContentType );
        }

        [Fact( DisplayName = "http head should return media resource content type and length only" )]
        public async Task HttpHeadShouldReturnMediaResourceContentTypeAndLengthOnly()
        {
            // arrange
            var request = NewHeadRequest( "api/people(1)/$value" );

            // act
            var response = await SendAsync( request );

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( "bytes", response.Headers.AcceptRanges.Single() );
            Assert.Equal( 500L, response.Content.Headers.ContentLength.Value );
            Assert.Equal( "image/png", response.Content.Headers.ContentType.MediaType );
        }

        [Fact( DisplayName = "http get should return media resource content" )]
        public async Task HttpGetShouldReturnMediaResourceContent()
        {
            // arrange
            var request = NewGetRequest( "api/people(2)/$value" );

            // act
            var response = await SendAsync( request );
            var stream = await response.Content.ReadAsStreamAsync();

            // assert
            Assert.Equal( OK, response.StatusCode );
            Assert.Equal( "bytes", response.Headers.AcceptRanges.Single() );
            Assert.Equal( 100L, response.Content.Headers.ContentLength.Value );
            Assert.Equal( "image/jpg", response.Content.Headers.ContentType.MediaType );
            Assert.Equal( 100L, stream.Length );
        }

        [Fact( DisplayName = "http get with range header should return partial media resource content" )]
        public async Task HttpGetWithRangeHeaderShouldReturnPartialMediaResourceContent()
        {
            // arrange
            var request = NewGetRequest( "api/people(2)/$value" );

            request.Headers.Range = new RangeHeaderValue( 0L, 49L );

            // act
            var response = await SendAsync( request );
            var stream = await response.Content.ReadAsStreamAsync();

            // assert
            Assert.Equal( PartialContent, response.StatusCode );
            Assert.Equal( "bytes", response.Headers.AcceptRanges.Single() );
            Assert.Equal( 50L, response.Content.Headers.ContentLength.Value );
            Assert.Equal( "image/jpg", response.Content.Headers.ContentType.MediaType );
            Assert.Equal( 50L, stream.Length );
        }
    }
}