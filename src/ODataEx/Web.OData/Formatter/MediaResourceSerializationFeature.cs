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
        /// Applies the serialization feature to the specified OData entry using the provided context.
        /// </summary>
        /// <param name="entry">The <see cref="ODataEntry"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public virtual void Apply( ODataEntry entry, ODataSerializationFeatureContext context )
        {
            Arg.NotNull( entry, nameof( entry ) );
            Arg.NotNull( context, nameof( context ) );

            var entityContext = context.EntityInstanceContext;
            var entityType = entityContext.EntityType;

            if ( !entityType.HasStream )
                return;

            if ( entityContext.SerializerContext.MetadataLevel == NoMetadata )
                return;

            var instance = entityContext.TryGetEntityInstance();

            if ( instance == null )
                return;

            var annotation = entityContext.EdmModel.GetAnnotationValue<MediaLinkEntryAnnotation>( entityType );

            if ( annotation == null )
                return;

            var contentType = annotation.GetContentType( instance );
            var readLink = annotation.GenerateReadLink?.Invoke( entityContext );
            var editLink = annotation.GenerateEditLink?.Invoke( entityContext );

            if ( string.IsNullOrEmpty( contentType ) || ( readLink == null && editLink == null ) )
                return;

            var mediaResource = new ODataStreamReferenceValue()
            {
                ContentType = contentType,
                ReadLink = readLink,
                EditLink = editLink,
                ETag = annotation.GenerateETag?.Invoke( entityContext )
            };

            entry.MediaResource = mediaResource;
        }

        void IODataSerializationFeature.Apply( ODataFeed feed, ODataSerializationFeatureContext context )
        {
        }

        void IODataSerializationFeature.Apply( ODataComplexValue complexValue, ODataSerializationFeatureContext context )
        {
        }
    }
}
