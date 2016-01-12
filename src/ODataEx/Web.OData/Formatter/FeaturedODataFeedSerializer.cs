namespace More.Web.OData.Formatter
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Web.OData.Formatter.Serialization;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;

    /// <summary>
    /// Represents a <see cref="ODataFeedSerializer">feed serializer</see> that supports customizable serialization features.
    /// </summary>
    public class FeaturedODataFeedSerializer : ODataFeedSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataFeedSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The underlying <see cref="ODataSerializerProvider">serializer provider</see>.</param>
        public FeaturedODataFeedSerializer( ODataSerializerProvider serializerProvider )
            : base( serializerProvider )
        {
        }

        /// <summary>
        /// Gets a list of serialization features for the serializer.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see>.</value>
        public IList<IODataSerializationFeature> SerializationFeatures
        {
            get;
        } = new List<IODataSerializationFeature>();

        /// <summary>
        /// Creates and returns an OData feed from the specified instance, type, and context.
        /// </summary>
        /// <param name="feedInstance">The sequence to create a feed for.</param>
        /// <param name="feedType">The feed collection type.</param>
        /// <param name="writeContext">The current <see cref="ODataSerializerContext">serializer context</see>.</param>
        /// <returns>The created <see cref="ODataFeed">feed</see>.</returns>
        public override ODataFeed CreateODataFeed( IEnumerable feedInstance, IEdmCollectionTypeReference feedType, ODataSerializerContext writeContext )
        {
            var feed = base.CreateODataFeed( feedInstance, feedType, writeContext );
            //var context = new ODataEntrySerializationContext( feed, entityInstanceContext, complexSerializer );

            //foreach ( var feature in SerializationFeatures )
            //    feature.Apply( context );

            return feed;
        }
    }
}
