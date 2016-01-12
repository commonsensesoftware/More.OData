namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;
    using static Microsoft.OData.Edm.EdmTypeKind;

    /// <summary>
    /// Represents an <see cref="ODataSerializerProvider">OData serializer provider</see> that support various serialization features.
    /// </summary>
    public class FeaturedODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly FeaturedODataFeedSerializer feedSerializer;
        private readonly FeaturedODataEntityTypeSerializer entityTypeSerializer;
        private readonly FeaturedODataComplexTypeSerializer complexTypeSerializer;
        private readonly ODataSerializationFeatureCollection serializationFeatures;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataSerializerProvider"/> class.
        /// </summary>
        public FeaturedODataSerializerProvider()
        {
            feedSerializer = new FeaturedODataFeedSerializer( this );
            entityTypeSerializer = new FeaturedODataEntityTypeSerializer( this );
            complexTypeSerializer = new FeaturedODataComplexTypeSerializer( this );
            serializationFeatures = new ODataSerializationFeatureCollection(
                feedSerializer.SerializationFeatures,
                entityTypeSerializer.SerializationFeatures,
                complexTypeSerializer.SerializationFeatures );
        }

        /// <summary>
        /// Gets a list of serialization features for the provider.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see>.</value>
        public IList<IODataSerializationFeature> SerializationFeatures
        {
            get
            {
                Contract.Ensures( Contract.Result<IList<IODataSerializationFeature>>() != null );
                return serializationFeatures;
            }
        }

        /// <summary>
        /// Returns the serializer for the given type.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference">EDM type reference</see> to get a serializer for.</param>
        /// <returns>A <see cref="ODataEdmTypeSerializer"/> for the given <paramref name="edmType">EDM type</paramref>.</returns>
        public override ODataEdmTypeSerializer GetEdmTypeSerializer( IEdmTypeReference edmType )
        {
            Contract.Assume( edmType != null );

            switch ( edmType.TypeKind() )
            {
                case Entity:
                    {
                        return entityTypeSerializer;
                    }
                case Complex:
                    {
                        return complexTypeSerializer;
                    }
                case Collection:
                    {
                        var type = edmType.AsCollection();

                        if ( !type.Definition.IsDeltaFeed() )
                        {
                            if ( type.ElementType().IsEntity() )
                                return feedSerializer;
                        }

                        break;
                    }
            }

            return base.GetEdmTypeSerializer( edmType );
        }
    }
}
