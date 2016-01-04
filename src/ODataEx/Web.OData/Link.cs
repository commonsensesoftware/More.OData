namespace More.Web.OData
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a link relation that service client can use to follow.
    /// </summary>
    /// <remarks>This class can be used to support Hypermedia as the Engine of Application State (HATEOAS).</remarks>
    public class Link
    {
        private readonly string relation;
        private readonly string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="Link"/> class.
        /// </summary>
        /// <param name="relation">The name of the relation.</param>
        /// <param name="url">The <see cref="Uri">URL</see> the link represents.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public Link( string relation, Uri url )
        {
            Arg.NotNullOrEmpty( relation, nameof( relation ) );
            Arg.NotNull( url, nameof( url ) );

            this.relation = relation;
            this.url = url.OriginalString;
        }

        /// <summary>
        /// Gets the name of the relation.
        /// </summary>
        /// <value>The name of the relation. The name can be represented as a
        /// Uniform Resource Indicator (URI).</value>
        public virtual string Relation
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( relation ) );
                return relation;
            }
        }

        /// <summary>
        /// Gets the URL the link represents.
        /// </summary>
        /// <value>The Uniform Resource Locator (URL) the link represents.</value>
        [SuppressMessage( "Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Serialization mechanisms do not support System.Uri." )]
        public virtual string Url
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( url ) );
                return url;
            }
        }
    }
}
