namespace System.Web.Http
{
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Http;
    using IO;
    using Linq;
    using Microsoft.OData.Core;
    using More;
    using More.Web.OData;
    using Net;
    using Net.Http;
    using Net.Http.Headers;
    using Results;
    using static More.ExceptionMessage;
    using static System.Globalization.CultureInfo;
    using static System.Net.Mime.MediaTypeNames.Application;

    /// <summary>
    /// Provides extension methods for <see cref="ApiController"/>.
    /// </summary>
    public static class ApiControllerExtensions
    {
        private static byte[] EmptyContent = new byte[0];

        /// <summary>
        /// Returns HTTP status code 200 (OK) for the specified query results.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="results">The <see cref="IQueryable">query</see> results.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="results"/>.</returns>
        /// <remarks>This extension method addresses a known issue where the <see cref="IQueryable"/> results may not
        /// correctly negotiate the entity model and media type formatter.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult Success( this ApiController controller, IQueryable results )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Arg.NotNull( results, nameof( results ) );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );

            var response = controller.Request.CreateResponse( HttpStatusCode.OK, results.GetType(), results );
            return new ResponseMessageResult( response );
        }

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 404 (Not Found) for the specified result.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="result">The resultant object.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="result"/>
        /// If the <paramref name="result"/> is <c>null</c>, HTTP status code 404 (Not Found) is returned;
        /// otherwise, HTTP status code 200 (OK) is returned.</returns>
        /// <remarks>This extension method addresses a known issue where the <see cref="IQueryable"/> results may not
        /// correctly negotiate the entity model and media type formatter.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult SuccessOrNotFound( this ApiController controller, object result )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );

            if ( result == null )
                return new NotFoundResult( controller );

            var response = controller.Request.CreateResponse( HttpStatusCode.OK, result.GetType(), result );
            return new ResponseMessageResult( response );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The stream must be disposed by the caller." )]
        private static Stream EnsureStreamCanSeek( Stream stream )
        {
            Contract.Requires( stream != null );
            Contract.Ensures( Contract.Result<Stream>() != null );
            Contract.Ensures( Contract.Result<Stream>().CanSeek );

            // stream is seekable
            if ( stream.CanSeek )
                return stream;

            // stream is not seekable, so copy it into a memory stream so we can seek on it
            var copy = new MemoryStream();

            stream.CopyTo( copy );
            stream.Dispose();
            copy.Flush();
            copy.Seek( 0L, SeekOrigin.Begin );

            return copy;
        }

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="content">The binary content to return.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="content"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range. The
        /// media type associated with the stream is always "application/octet-stream".</remarks>
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the response." )]
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, byte[] content ) =>
            controller.SuccessOrPartialContent( new MemoryStream( content ?? EmptyContent, false ), new MediaTypeHeaderValue( Octet ) );

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="stream">The <see cref="Stream">stream</see> to return.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="stream"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range. The
        /// media type associated with the stream is always "application/octet-stream".</remarks>
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, Stream stream ) =>
            controller.SuccessOrPartialContent( stream, new MediaTypeHeaderValue( Octet ) );

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="content">The binary content to return.</param>
        /// <param name="mediaType">The media content type associated with the content.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="content"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range.</remarks>
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the response." )]
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, byte[] content, string mediaType ) =>
            controller.SuccessOrPartialContent( new MemoryStream( content ?? EmptyContent, false ), new MediaTypeHeaderValue( mediaType.OrWhenNullOrEmpty( Octet ) ) );

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="stream">The <see cref="Stream">stream</see> to return.</param>
        /// <param name="mediaType">The media content type associated with the stream.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="stream"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range.</remarks>
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, Stream stream, string mediaType ) =>
            controller.SuccessOrPartialContent( stream, new MediaTypeHeaderValue( mediaType.OrWhenNullOrEmpty( Octet ) ) );
        
        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="content">The binary content to return.</param>
        /// <param name="mediaType">The content <see cref="MediaTypeHeaderValue">media type</see>.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="content"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the response." )]
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, byte[] content, MediaTypeHeaderValue mediaType ) =>
            controller.SuccessOrPartialContent( new MemoryStream( content ?? EmptyContent, false ), mediaType );

        /// <summary>
        /// Returns HTTP status code 200 (OK) or 206 (Partial Content) if the client requested the "Range" header and the range can be satisfied.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="stream">The <see cref="Stream">stream</see> to return.</param>
        /// <param name="mediaType">The <paramref name="stream"/> <see cref="MediaTypeHeaderValue">media type</see>.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> for the specified <paramref name="stream"/>.</returns>
        /// <remarks>This method will return HTTP status code 416 (Requested Range Not Satisfiable) if the client specifies an invalid range.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult SuccessOrPartialContent( this ApiController controller, Stream stream, MediaTypeHeaderValue mediaType )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Arg.NotNull( stream, nameof( stream ) );
            Arg.NotNull( mediaType, nameof( mediaType ) );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );

            // get the range and stream media type
            var request = controller.Request;
            var headers = request.Headers;
            var range = headers.Range;
            HttpResponseMessage response;

            if ( range == null )
            {
                // if the range header is present but null, then the header value must be invalid
                if ( headers.Contains( "Range" ) )
                {
                    var error = new ODataError { Message = string.Format( CurrentCulture, ControllerRangeNotSatisfiable, stream.Length - 1L ) };
                    return new ResponseMessageResult( controller.Request.CreateResponse( HttpStatusCode.RequestedRangeNotSatisfiable, error ) );
                }

                // if no range was requested, return the entire stream
                response = request.CreateResponse( HttpStatusCode.OK );
                response.Headers.AcceptRanges.Add( "bytes" );
                response.Content = new StreamContent( stream );
                response.Content.Headers.ContentType = mediaType;

                return new ResponseMessageResult( response );
            }

            var partialStream = EnsureStreamCanSeek( stream );

            response = request.CreateResponse( HttpStatusCode.PartialContent );
            response.Headers.AcceptRanges.Add( "bytes" );

            try
            {
                // return the requested range(s)
                response.Content = new ByteRangeStreamContent( partialStream, range, mediaType );
            }
            catch ( InvalidByteRangeException ex )
            {
                response.Dispose();
                return new ResponseMessageResult( request.CreateErrorResponse( ex ) );
            }

            // change status code if the entire stream was requested
            if ( response.Content.Headers.ContentLength.Value == partialStream.Length )
                response.StatusCode = HttpStatusCode.OK;

            return new ResponseMessageResult( response );
        }

        /// <summary>
        /// Returns HTTP status code 200 (OK) with the specified values for the Content-Length and Content-Type headers.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="contentLength">The length of the content.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> result.</returns>
        /// <remarks>This method is typically used as the response to a HTTP HEAD request where the content headers should be returned without any content.
        /// This method always indicates that the media type is "application/octect-stream". This method also returns the Accept-Ranges header with a value
        /// of "bytes", which indicates the client can request partial content. Service authors can implement this behavior using any of the
        /// <see cref="SuccessOrPartialContent(ApiController, Stream, string)"/> methods when returning content.</remarks>
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult OkWithContentHeaders( this ApiController controller, long contentLength ) =>
            OkWithContentHeaders( controller, contentLength, Octet );

        /// <summary>
        /// Returns HTTP status code 200 (OK) with the specified values for the Content-Length and Content-Type headers.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="contentLength">The length of the content.</param>
        /// <param name="mediaType">The media type of the content. If this parameter is <c>null</c> or empty, then "application/octect-stream"
        /// will be assumed.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> result.</returns>
        /// <remarks>This method is typically used as the response to a HTTP HEAD request where the content headers should be returned without any content.
        /// This method also returns the Accept-Ranges header with a value of "bytes", which indicates the client can request partial content. Service authors
        /// can implement this behavior using any of the <see cref="SuccessOrPartialContent(ApiController, Stream, string)"/> methods when returning content.</remarks>
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult OkWithContentHeaders( this ApiController controller, long contentLength, string mediaType )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );
            Arg.GreaterThanOrEqualTo( contentLength, 0L, nameof( contentLength ) );

            // note: use an empty stream so that web api reports the correct "content-length" header
            // if you try to directly set content-length without content, it will always update to zero
            var response = new HttpResponseMessage( HttpStatusCode.OK );
            var content = new StreamContent( new EmptyStream( contentLength ) );

            content.Headers.ContentType = new MediaTypeHeaderValue( mediaType.OrWhenNullOrEmpty( Octet ) );
            response.Headers.AcceptRanges.Add( "bytes" );
            response.Content = content;

            return new ResponseMessageResult( response );
        }

        /// <summary>
        /// Returns HTTP status code 501 (Not Implemented) for the specific result.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> representing HTTP status code 501 (Not Implemented).</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult NotImplemented( this ApiController controller )
        {
            var error = new ODataError() { Message = string.Format( CurrentCulture, ControllerUnsupportedMethod, controller.Request.Method ) };
            return new ResponseMessageResult( controller.Request.CreateResponse( HttpStatusCode.NotImplemented, error ) );
        }

        /// <summary>
        /// Returns HTTP status code 501 (Not Implemented) for the specific result.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="message">The associated error message.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> representing HTTP status code 501 (Not Implemented).</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult NotImplemented( this ApiController controller, string message )
        {
            Arg.NotNullOrEmpty( message, nameof( message ) );
            var error = new ODataError() { Message = message };
            return new ResponseMessageResult( controller.Request.CreateResponse( HttpStatusCode.NotImplemented, error ) );
        }

        /// <summary>
        /// Returns HTTP status code 501 (Not Implemented) for the specific result.
        /// </summary>
        /// <param name="controller">The extended <see cref="ApiController">controller</see> .</param>
        /// <param name="error">The returned <see cref="ODataError">error</see>.</param>
        /// <returns>The <see cref="IHttpActionResult">response</see> representing HTTP status code 501 (Not Implemented).</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed by the caller." )]
        public static IHttpActionResult NotImplemented( this ApiController controller, ODataError error )
        {
            Arg.NotNull( controller, nameof( controller ) );
            Arg.NotNull( error, nameof( error ) );
            Contract.Ensures( Contract.Result<IHttpActionResult>() != null );
            return new ResponseMessageResult( controller.Request.CreateResponse( HttpStatusCode.NotImplemented, error ) );
        }
    }
}
