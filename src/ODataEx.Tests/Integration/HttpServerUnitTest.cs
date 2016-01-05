namespace More.Integration
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    /// <summary>
    /// Represents the base implementation for <see cref="HttpServer">HTTP server</see> tests.
    /// </summary>
    public abstract class HttpServerUnitTest : IDisposable
    {
        private readonly HttpMessageInvoker invoker;
        private bool disposed;

        /// <summary>
        /// Releases the managed and unmanaged resources used by the <see cref="HttpServerUnitTest"/> class.
        /// </summary>
        ~HttpServerUnitTest()
        {
            Dispose( false );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServerUnitTest"/> class.
        /// </summary>
        /// <param name="initialize">The initialization <see cref="Action">action</see> to perform when the test is created.</param>
        protected HttpServerUnitTest( Action<HttpConfiguration> initialize )
        {
            Contract.Requires( initialize != null );

            var configuration = new HttpConfiguration();
            initialize( configuration );
            invoker = new HttpMessageInvoker( new HttpServer( configuration ) );
        }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="HttpServerUnitTest"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
                return;

            disposed = true;

            if ( !disposing )
                return;

            invoker.Dispose();
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="HttpServerUnitTest"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Sends the specified request asynchronously.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to send.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="HttpResponseMessage">response</see>.</returns>
        protected Task<HttpResponseMessage> SendAsync( HttpRequestMessage request )
        {
            Contract.Requires( request != null );
            Contract.Ensures( Contract.Result<Task<HttpResponseMessage>>() != null );
            return invoker.SendAsync( request, CancellationToken.None );
        }

        /// <summary>
        /// Converts the specified object to its equivalent Java Script Object Notation (JSON) form.
        /// </summary>
        /// <typeparam name="TObject">The <see cref="Type">type</see> of object to convert.</typeparam>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The <paramref name="obj">object</paramref> in the JSON format.</returns>
        protected async Task<string> ToJson<TObject>( TObject obj )
        {
            Contract.Requires( obj != null );
            Contract.Ensures( Contract.Result<Task<string>>() != null );

            var formatter = new JsonMediaTypeFormatter();

            using ( var content = new ObjectContent<TObject>( obj, formatter ) )
                return await content.ReadAsStringAsync();
        }

        private async Task<HttpRequestMessage> CreateRequestAsync<TObject>( HttpMethod method, string requestUri, TObject obj )
        {
            Contract.Requires( method != null );
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<Task<HttpResponseMessage>>() != null );

            var url = new Uri( requestUri, UriKind.RelativeOrAbsolute );

            if ( !url.IsAbsoluteUri )
                url = new Uri( new Uri( "http://localhost" ), url );

            var request = new HttpRequestMessage( method, url );

            request.Headers.Add( "Host", "localhost" );

            if ( Equals( obj, default( TObject ) ) )
                return request;

            var json = await ToJson( obj );
            var content = new StringContent( json, Encoding.UTF8, "application/json" );

            request.Content = content;

            return request;
        }

        /// <summary>
        /// Creates a HTTP GET request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">HTTP request</see>.</returns>
        protected async Task<HttpRequestMessage> CreateGetRequestAsync( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );

            var request = await CreateRequestAsync( HttpMethod.Get, requestUri, default( object ) );
            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
            return request;
        }

        /// <summary>
        /// Creates a HTTP POST request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <param name="obj">The <typeparam name="TObject">object</typeparam> representing the body sent in the request.</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">HTTP request</see>.</returns>
        protected Task<HttpRequestMessage> CreatePostRequestAsync<TObject>( string requestUri, TObject obj )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Requires( obj != null );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );
            return CreateRequestAsync( HttpMethod.Post, requestUri, obj );
        }

        /// <summary>
        /// Creates a HTTP PUT request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <param name="obj">The <typeparam name="TObject">object</typeparam> representing the body sent in the request.</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">HTTP request</see>.</returns>
        protected Task<HttpRequestMessage> CreatePutRequestAsync<TObject>( string requestUri, TObject obj )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Requires( obj != null );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );
            return CreateRequestAsync( HttpMethod.Put, requestUri, obj );
        }

        /// <summary>
        /// Creates a HTTP PATCH request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <param name="obj">The <typeparam name="TObject">object</typeparam> representing the body sent in the request.</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">HTTP request</see>.</returns>
        protected Task<HttpRequestMessage> CreatePatchRequestAsync<TObject>( string requestUri, TObject obj )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Requires( obj != null );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );
            return CreateRequestAsync( new HttpMethod( "PATCH" ), requestUri, obj );
        }

        /// <summary>
        /// Creates a HTTP DELETE request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">HTTP request</see>.</returns>
        protected Task<HttpRequestMessage> CreateDeleteRequestAsync( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );
            return CreateRequestAsync( HttpMethod.Delete, requestUri, default( object ) );
        }
    }
}
