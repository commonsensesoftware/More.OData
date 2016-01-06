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
        private string name;
        private string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="Link"/> class.
        /// </summary>
        /// <param name="name">The name of the link.</param>
        /// <param name="url">The <see cref="Uri">URL</see> the link represents.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public Link( string name, Uri url )
        {
            Arg.NotNullOrEmpty( name, nameof( name ) );
            Arg.NotNull( url, nameof( url ) );

            this.name = name;
            this.url = url.OriginalString;
        }

        /// <summary>
        /// Gets or sets the name of the link.
        /// </summary>
        /// <value>The name of the link.</value>
        public string Name
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( name ) );
                return name;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL the link represents.
        /// </summary>
        /// <value>The Uniform Resource Locator (URL) the link represents.</value>
        [SuppressMessage( "Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Serialization mechanisms do not support System.Uri." )]
        public string Url
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( url ) );
                return url;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                url = new Uri( value ).OriginalString;
            }
        }
    }
}
