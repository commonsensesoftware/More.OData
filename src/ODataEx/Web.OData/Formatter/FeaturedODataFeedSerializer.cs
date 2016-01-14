using System.Diagnostics.Contracts;
namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents a <see cref="ODataFeedSerializer">feed serializer</see> that supports customizable serialization features.
    /// </summary>
    public class FeaturedODataFeedSerializer : ODataFeedSerializer
    {
        private readonly ODataComplexTypeSerializer complexSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataFeedSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The underlying <see cref="ODataSerializerProvider">serializer provider</see>.</param>
        /// <param name="serializationFeatures">The <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see> associated with the serializer.</param>
        public FeaturedODataFeedSerializer( ODataSerializerProvider serializerProvider, IList<IODataSerializationFeature> serializationFeatures )
            : base( serializerProvider )
        {
            Arg.NotNull( serializationFeatures, nameof( serializationFeatures ) );
            complexSerializer = new ODataComplexTypeSerializer( serializerProvider );
            SerializationFeatures = serializationFeatures;
        }

        /// <summary>
        /// Gets the complex type serializer associated with the serializer.
        /// </summary>
        /// <value>The associated <see cref="ODataComplexTypeSerializer">complex type serializer</see>.</value>
        protected ODataComplexTypeSerializer ComplexTypeSerializer
        {
            get
            {
                Contract.Ensures( complexSerializer != null );
                return complexSerializer;
            }
        }

        /// <summary>
        /// Gets a list of serialization features for the serializer.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see>.</value>
        public IList<IODataSerializationFeature> SerializationFeatures
        {
            get;
        }

        /// <summary>
        /// Creates and returns an OData feed from the specified instance, type, and context.
        /// </summary>
        /// <param name="feedInstance">The sequence to create a feed for.</param>
        /// <param name="feedType">The feed collection type.</param>
        /// <param name="writeContext">The current <see cref="ODataSerializerContext">serializer context</see>.</param>
        /// <returns>The created <see cref="ODataFeed">feed</see>.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "Validated by a code contract." )]
        public override ODataFeed CreateODataFeed( IEnumerable feedInstance, IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext )
        {
            Contract.Assume( writeContext != null );

            var feed = base.CreateODataFeed( feedInstance, feedType, writeContext );
            var entitySet = writeContext.Model.EntityContainer.FindEntitySet( writeContext.NavigationSource.Name );
            var context = new ODataSerializationFeatureContext( entitySet, writeContext, ComplexTypeSerializer ) { Instance = feedInstance };

            foreach ( var feature in SerializationFeatures )
                feature.Apply( feed, context );

            return feed;
        }
    }
}
