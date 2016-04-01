namespace More.Web.OData
{
    using Microsoft.OData.Core;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Results;

    /// <summary>
    /// Represents an <see cref="IHttpActionResult">action result</see> that returns a <see cref="HttpStatusCode.BadRequest"/>
    /// response when invalid OData query options are requested.
    /// </summary>
    public class ODataQueryValidationResult : BadRequestErrorMessageResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryValidationResult"/> class.
        /// </summary>
        /// <param name="error">The <see cref="ODataException">error</see> associated with the result.</param>
        /// <param name="controller">The associated <see cref="ApiController">controller</see>.</param>
        public ODataQueryValidationResult( ODataException error, ApiController controller )
            : base( error?.Message ?? string.Empty, controller )
        {
            IsValid = error == null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryValidationResult"/> class.
        /// </summary>
        /// <param name="error">The <see cref="ODataException">error</see> associated with the result.</param>
        /// <param name="contentNegotiator">The <see cref="IContentNegotiator">content negotiator</see> used by the result.</param>
        /// <param name="request">The associated <see cref="HttpRequestMessage">request</see>.</param>
        /// <param name="formatters">The <see cref="IEnumerable{T}">sequence</see> of <see cref="MediaTypeFormatter">media type formatters</see>
        /// used by the result.</param>
        public ODataQueryValidationResult( ODataException error, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters )
            : base( error?.Message ?? string.Empty, contentNegotiator, request, formatters )
        {
            IsValid = error == null;
        }

        /// <summary>
        /// Gets a value indicating whether the result is valid.
        /// </summary>
        /// <value>True if the result is valid; otherwise, false.</value>
        public bool IsValid { get; }
    }
}
