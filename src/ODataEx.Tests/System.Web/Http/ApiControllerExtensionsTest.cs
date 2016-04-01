using Xunit;
namespace System.Web.Http
{
    using Microsoft.OData.Core;
    using Moq;
    using Net;
    using Net.Http;
    using Net.Http.Headers;
    using OData;
    using OData.Query;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Threading;
    using Xunit;

    /// <summary>
    /// Provides unit tests for <see cref="ApiControllerExtensions"/>.
    /// </summary>
    public class ApiControllerExtensionsTest
    {
        [Fact( DisplayName = "success should return http 200" )]
        public async Task SuccessShouldReturnHttp200()
        {
            // arrange
            IQueryable results = new[] { new object() }.AsQueryable();
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.Success( results );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
        }

        [Fact( DisplayName = "success or not found should return http 200 when result is non-null" )]
        public async Task SuccessOrFoundShouldReturnHttp200WhenResultIsNonNull()
        {
            // arrange
            var result = new object();
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrNotFound( result );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
        }

        [Fact( DisplayName = "success or not found should return http 404 when result is null" )]
        public async Task SuccessOrFoundShouldReturnHttp404WhenResultIsNull()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrNotFound( null );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.NotFound, response.StatusCode );
        }

        [Fact( DisplayName = "success or partial content should return http 200 when all content is returned" )]
        public async Task SuccessOrPartialContentShouldReturnHttp200WhenAllContentIsReturned()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)/$value" );
            var controller = new Mock<ApiController>().Object;
            var stream = new MemoryStream( new byte[100] );

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrPartialContent( stream );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( "bytes", response.Headers.AcceptRanges.Single() );
            Assert.Equal( 100L, response.Content.Headers.ContentLength.Value );
            Assert.Equal( "application/octet-stream", response.Content.Headers.ContentType.MediaType );
        }

        [Fact( DisplayName = "success or partial content should return http 206 when partial content is returned" )]
        public async Task SuccessOrPartialContentShouldReturnHttp206WhenPartialContentIsReturned()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)/$value" );
            var controller = new Mock<ApiController>().Object;
            var stream = new MemoryStream( new byte[100] );
            var mediaType = "application/vnd.test";

            request.SetConfiguration( configuration );
            request.Headers.Range = new RangeHeaderValue( 0, 49 );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrPartialContent( stream, mediaType );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.PartialContent, response.StatusCode );
            Assert.Equal( "bytes", response.Headers.AcceptRanges.Single() );
            Assert.Equal( 50L, response.Content.Headers.ContentLength.Value );
            Assert.Equal( mediaType, response.Content.Headers.ContentType.MediaType );
        }

        [Fact( DisplayName = "success or partial content should return http 200 when range equals entire content" )]
        public async Task SuccessOrPartialContentShouldReturnHttp200WhenRangeEqualsEntireContent()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)/$value" );
            var controller = new Mock<ApiController>().Object;
            var stream = new MemoryStream( new byte[100] );

            request.SetConfiguration( configuration );
            request.Headers.Range = new RangeHeaderValue( 0, 99 );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrPartialContent( stream );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( 100L, response.Content.Headers.ContentLength.Value );
        }

        [Fact( DisplayName = "success or partial content should return http 416 when range is invalid" )]
        public async Task SuccessOrPartialContentShouldReturnHttp416WhenRangeIsInvalid()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Get, "http://localhost/Tests(1)/$value" );
            var controller = new Mock<ApiController>().Object;
            var stream = new MemoryStream();

            request.SetConfiguration( configuration );
            request.Headers.TryAddWithoutValidation( "Range", "custom=abc" );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.SuccessOrPartialContent( stream );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );

            // assert
            Assert.Equal( HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode );
        }

        [Fact( DisplayName = "not implemented should return http 501" )]
        public async Task NotImplementedShouldReturnDefaultError()
        {
            // arrange
            var expected = new ODataError() { Message = "DELETE requests are not supported." };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Delete, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.NotImplemented();
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = await response.Content.ReadAsAsync<ODataError>();

            // assert
            Assert.Equal( HttpStatusCode.NotImplemented, response.StatusCode );
            Assert.Equal( expected.ErrorCode, actual.ErrorCode );
            Assert.Equal( expected.Message, actual.Message );
        }

        [Fact( DisplayName = "not implemented should return http 501 with custom message" )]
        public async Task NotImplementedShouldReturnErrorWithCustomMessage()
        {
            // arrange
            var expected = new ODataError() { Message = "The OPTIONS method is unsupported." };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.NotImplemented( expected );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = await response.Content.ReadAsAsync<ODataError>();

            // assert
            Assert.Equal( HttpStatusCode.NotImplemented, response.StatusCode );
            Assert.Equal( expected.ErrorCode, actual.ErrorCode );
            Assert.Equal( expected.Message, actual.Message );
        }

        [Fact( DisplayName = "not implemented should return http 501 custom error" )]
        public async Task NotImplementedShouldReturnCustomError()
        {
            // arrange
            var expected = new ODataError()
            {
                ErrorCode = "42",
                Message = "The OPTIONS method is unsupported."
            };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.NotImplemented( expected );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = await response.Content.ReadAsAsync<ODataError>();

            // assert
            Assert.Equal( HttpStatusCode.NotImplemented, response.StatusCode );
            Assert.Equal( expected.ErrorCode, actual.ErrorCode );
            Assert.Equal( expected.Message, actual.Message );
        }

        [Fact( DisplayName = "no content should return http 204" )]
        public async Task NoContentShouldReturnHttp204()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;
            var expected = HttpStatusCode.NoContent;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.NoContent();
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = response.StatusCode;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "precondition failed should return http 412" )]
        public async Task PreconditionFailedShouldReturnHttp412()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;
            var expected = HttpStatusCode.PreconditionFailed;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.PreconditionFailed();
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = response.StatusCode;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "precondition failed should return http 412 with custom error" )]
        public async Task PreconditionFailedShouldReturnHttp412WithCustomError()
        {
            // arrange
            var expected = new ODataError()
            {
                ErrorCode = "42",
                Message = "The ETag specified in the If-Match header is no longer valid."
            };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.PreconditionFailed( expected );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = await response.Content.ReadAsAsync<ODataError>();

            // assert
            Assert.Equal( HttpStatusCode.PreconditionFailed, response.StatusCode );
            Assert.Equal( expected.ErrorCode, actual.ErrorCode );
            Assert.Equal( expected.Message, actual.Message );
        }

        [Fact( DisplayName = "precondition required should return http 428" )]
        public async Task PreconditionRequiredShouldReturnHttp428()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;
            var expected = (HttpStatusCode) 428;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.PreconditionRequired();
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = response.StatusCode;

            // assert
            Assert.Equal( expected, actual );
        }

        [Fact( DisplayName = "precondition required should return http 428 with custom error" )]
        public async Task PreconditionRequiredShouldReturnHttp428WithCustomError()
        {
            // arrange
            var expected = new ODataError()
            {
                ErrorCode = "42",
                Message = "The If-Match header must be specified."
            };
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage( HttpMethod.Options, "http://localhost/Tests(1)" );
            var controller = new Mock<ApiController>().Object;

            request.SetConfiguration( configuration );
            controller.Request = request;
            controller.Configuration = new HttpConfiguration();

            // act
            var actionResult = controller.PreconditionRequired( expected );
            var response = await actionResult.ExecuteAsync( CancellationToken.None );
            var actual = await response.Content.ReadAsAsync<ODataError>();

            // assert
            Assert.Equal( (HttpStatusCode) 428, response.StatusCode );
            Assert.Equal( expected.ErrorCode, actual.ErrorCode );
            Assert.Equal( expected.Message, actual.Message );
        }
    }
}
