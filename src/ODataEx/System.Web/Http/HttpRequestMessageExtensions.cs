namespace System.Web.Http
{
    using Diagnostics.Contracts;
    using Linq.Expressions;
    using Net;
    using Net.Http;
    using Reflection;
    using System;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private static readonly Lazy<MethodInfo> untypedCreateResponse = new Lazy<MethodInfo>( CreateUntypedCreateResponseMethod );

        private static MethodInfo CreateUntypedCreateResponseMethod()
        {
            Contract.Ensures( Contract.Result<MethodInfo>() != null );
            Expression<Func<HttpRequestMessage, HttpResponseMessage>> expression = request => request.CreateResponse( HttpStatusCode.OK, default( object ) );
            var body = (MethodCallExpression) expression.Body;
            return body.Method.GetGenericMethodDefinition();
        }

        /// <summary>
        /// Creates a HTTP response message with the specified status, type, and value.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to create a response for.</param>
        /// <param name="status">The <see cref="HttpStatusCode">status</see> of the response.</param>
        /// <param name="type">The <see cref="Type">type</see> of value in the response.</param>
        /// <param name="value">The response value.</param>
        /// <returns>A <see cref="HttpResponseMessage">response message</see>.</returns>
        /// <remarks>Content negotiation doesn't use the runtime type of the object. This passes the specified type to content negotiation.</remarks>
        public static HttpResponseMessage CreateResponse( this HttpRequestMessage request, HttpStatusCode status, Type type, object value )
        {
            Arg.NotNull( request, nameof( request ) );
            Arg.NotNull( type, nameof( type ) );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            // HACK: addresses known bug where the media type formatters do not correctly setup the response for an untyped object.
            // this behavior most commonly occurs when the returned object is the result of a query projection.
            var method = untypedCreateResponse.Value.MakeGenericMethod( type );
            var args = new[] { request, status, value };
            return (HttpResponseMessage) method.Invoke( null, args );
        }
    }
}
