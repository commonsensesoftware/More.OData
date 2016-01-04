namespace More.OData.Edm
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.OData;

    /// <summary>
    /// Represents an annotation for a Media Link Entry (MLE).
    /// </summary>
    public class MediaLinkEntryAnnotation
    {
        private readonly Lazy<Func<object, string>> contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLinkEntryAnnotation"/> class.
        /// </summary>
        /// <param name="contentType">The <see cref="Func{T, TResult}">method</see> used to retrieve the content type for the media
        /// resource provided by an entity instance</param>
        public MediaLinkEntryAnnotation( Func<object, string> contentType )
        {
            Arg.NotNull( contentType, nameof( contentType ) );
            this.contentType = new Lazy<Func<object, string>>( () => contentType );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaLinkEntryAnnotation"/> class.
        /// </summary>
        /// <param name="contentType">The <see cref="Lazy{T}">lazy-initialized</see> <see cref="Func{T, TResult}">method</see> used
        /// to retrieve the content type for the media resource provided by an entity instance</param>
        [SuppressMessage( "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Required for generic, lazy func." )]
        public MediaLinkEntryAnnotation( Lazy<Func<object, string>> contentType )
        {
            Arg.NotNull( contentType, nameof( contentType ) );
            this.contentType = contentType;
        }

        /// <summary>
        /// Gets the content type associated with the media link entry for the media resource provided by the specified entity instance.
        /// </summary>
        /// <param name="instance">The instance of the entity to get the content type from.</param>
        /// <returns>The associated media content type.</returns>
        /// <exception cref="InvalidCastException">The specified <paramref name="instance"/> is not the configured entity type.</exception>
        public string GetContentType( object instance )
        {
            Arg.NotNull( instance, nameof( instance ) );
            return contentType.Value( instance );
        }

        /// <summary>
        /// Gets or sets the factory method used to generate read links.
        /// </summary>
        /// <value>The factory <see cref="Func{T, TResult}">method</see> used to generate read links.</value>
        /// <remarks>This property will be <c>null</c> if the media resource is write-only.</remarks>
        public Func<EntityInstanceContext, Uri> GenerateReadLink
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the factory method used to generate edit links.
        /// </summary>
        /// <value>The factory <see cref="Func{T, TResult}">method</see> used to generate edit links.</value>
        /// <remarks>This property will be <c>null</c> if the media resource is read-only.</remarks>
        public Func<EntityInstanceContext, Uri> GenerateEditLink
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the factory method used to generate entity tags for the media resource.
        /// </summary>
        /// <value>The factory <see cref="Func{T, TResult}">method</see> used to generate entity tags.
        /// This property can be <c>null</c>.</value>
        public Func<EntityInstanceContext, string> GenerateETag
        {
            get;
            set;
        }
    }
}
