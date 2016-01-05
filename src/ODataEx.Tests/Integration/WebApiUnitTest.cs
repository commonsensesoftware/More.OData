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
    /// Represents the base implementation to host and test an ASP.NET Web API service.
    /// </summary>
    public abstract class WebApiUnitTest : IDisposable
    {
        private readonly Lazy<HttpMessageInvoker> invoker;
        private bool disposed;

        /// <summary>
        /// Releases the managed and unmanaged resources used by the <see cref="WebApiUnitTest"/> class.
        /// </summary>
        ~WebApiUnitTest()
        {
            Dispose( false );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiUnitTest"/> class.
        /// </summary>
        protected WebApiUnitTest()
        {
            invoker = new Lazy<HttpMessageInvoker>( CreateInvoker );
        }

        /// <summary>
        /// Releases the managed and, optionally, the unmanaged resources used by the <see cref="WebApiUnitTest"/> class.
        /// </summary>
        /// <param name="disposing">Indicates whether the object is being disposed.</param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposed )
                return;

            disposed = true;

            if ( !disposing )
                return;

            if ( invoker.IsValueCreated )
                invoker.Value.Dispose();
        }

        /// <summary>
        /// Releases the managed resources used by the <see cref="WebApiUnitTest"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private HttpMessageInvoker CreateInvoker()
        {
            var configuration = new HttpConfiguration();
            Initialize( configuration );
            return new HttpMessageInvoker( new HttpServer( configuration ) );
        }

        /// <summary>
        /// Initializes the ASP.NET Web API configuration for the unit test.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration">configuration</see> to initialize with.</param>
        protected abstract void Initialize( HttpConfiguration configuration );

        /// <summary>
        /// Sends the specified request asynchronously.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to send.</param>
        /// <returns>A <see cref="Task{TResult}">task</see> containing the <see cref="HttpResponseMessage">response</see>.</returns>
        protected Task<HttpResponseMessage> SendAsync( HttpRequestMessage request )
        {
            Contract.Requires( request != null );
            Contract.Ensures( Contract.Result<Task<HttpResponseMessage>>() != null );
            return invoker.Value.SendAsync( request, CancellationToken.None );
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

        private HttpRequestMessage CreateRequest( HttpMethod method, string requestUri )
        {
            Contract.Requires( method != null );
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            var url = new Uri( requestUri, UriKind.RelativeOrAbsolute );

            if ( !url.IsAbsoluteUri )
                url = new Uri( new Uri( "http://localhost" ), url );

            var request = new HttpRequestMessage( method, url );

            request.Headers.Add( "Host", "localhost" );

            return request;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync<TObject>( HttpMethod method, string requestUri, TObject obj )
        {
            Contract.Requires( method != null );
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<Task<HttpResponseMessage>>() != null );

            var request = CreateRequest( method, requestUri );

            if ( Equals( obj, default( TObject ) ) )
                return request;

            var json = await ToJson( obj );
            var content = new StringContent( json, Encoding.UTF8, "application/json" );

            request.Content = content;

            return request;
        }

        /// <summary>
        /// Creates a HTTP GET request for the specified URL.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <returns>The created <see cref="HttpRequestMessage">request</see>.</returns>
        protected HttpRequestMessage NewGetRequest( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<Task<HttpRequestMessage>>() != null );

            var request = CreateRequest( HttpMethod.Get, requestUri );
            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
            return request;
        }

        /// <summary>
        /// Creates a HTTP POST request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <param name="obj">The <typeparam name="TObject">object</typeparam> representing the body sent in the request.</param>
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">request</see>.</returns>
        protected Task<HttpRequestMessage> NewPostRequestAsync<TObject>( string requestUri, TObject obj )
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
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">request</see>.</returns>
        protected Task<HttpRequestMessage> NewPutRequestAsync<TObject>( string requestUri, TObject obj )
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
        /// <returns>A <see cref="Task{T}">task</see> containing the created <see cref="HttpRequestMessage">request</see>.</returns>
        protected Task<HttpRequestMessage> NewPatchRequestAsync<TObject>( string requestUri, TObject obj )
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
        /// <returns>The created <see cref="HttpRequestMessage">request</see>.</returns>
        protected HttpRequestMessage NewDeleteRequest( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<HttpRequestMessage>() != null );
            return CreateRequest( HttpMethod.Delete, requestUri );
        }

        /// <summary>
        /// Creates a HTTP HEAD request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <returns>The created <see cref="HttpRequestMessage">request</see>.</returns>
        protected HttpRequestMessage NewHeadRequest( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<HttpRequestMessage>() != null );
            return CreateRequest( HttpMethod.Head, requestUri );
        }

        /// <summary>
        /// Creates a HTTP OPTIONS request for the specified URL asynchronously.
        /// </summary>
        /// <param name="requestUri">The request URL</param>
        /// <returns>The created <see cref="HttpRequestMessage">request</see>.</returns>
        protected HttpRequestMessage NewOptionsRequest( string requestUri )
        {
            Contract.Requires( !string.IsNullOrEmpty( requestUri ) );
            Contract.Ensures( Contract.Result<HttpRequestMessage>() != null );
            return CreateRequest( HttpMethod.Options, requestUri );
        }
    }
}
