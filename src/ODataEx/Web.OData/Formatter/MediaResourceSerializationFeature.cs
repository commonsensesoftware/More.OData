namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using More.OData.Edm;
    using System.Diagnostics.CodeAnalysis;
    using static System.Web.OData.Formatter.ODataMetadataLevel;

    /// <summary>
    /// Represents a serialization feature to generate media link entries (MLE) for OData media resoures.
    /// </summary>
    public class MediaResourceSerializationFeature : IODataSerializationFeature
    {
        /// <summary>
        /// Applies the serialization feature using the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ODataEntrySerializationContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataEntrySerializationContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var entityContext = context.EntityInstanceContext;
            var entityType = entityContext.EntityType;

            // if the model doesn't have a stream, then this isn't a media link entry
            if ( !entityType.HasStream )
                return;

            var serializerContext = entityContext.SerializerContext;

            // if no metadata was requested or we're inside a projection (e.g. $select), we don't build a reference
            if ( serializerContext.MetadataLevel == NoMetadata || serializerContext.SelectExpandClause != null )
                return;

            var instance = entityContext.EntityInstance;

            // must have an entity in order to generate a MLE
            if ( instance == null )
                return;

            // determine whether the media resource is configured via an annotation
            var annotation = entityContext.EdmModel.GetAnnotationValue<MediaLinkEntryAnnotation>( entityType );

            // the entry is not annotated, so we don't have what's need to build the reference
            if ( annotation == null )
                return;

            // get the content type from the current entity instance
            var contentType = annotation.GetContentType( instance );
            var readLink = annotation.GenerateReadLink?.Invoke( entityContext );
            var editLink = annotation.GenerateEditLink?.Invoke( entityContext );

            // if the content type is unspecified or we don't have a read or edit link, this either means the entity instance
            // doesn't have a media resource or there is an issue with the data; downgrade gracefully
            if ( string.IsNullOrEmpty( contentType ) || ( readLink == null && editLink == null ) )
                return;

            var mediaResource = new ODataStreamReferenceValue()
            {
                ContentType = contentType,
                ReadLink = readLink,
                EditLink = editLink,
                ETag = annotation.GenerateETag?.Invoke( entityContext )
            };

            context.Entry.MediaResource = mediaResource;
        }
    }
}
