﻿namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents an <see cref="ODataSerializerProvider">OData serializer provider</see> that support various serialization features.
    /// </summary>
    public class FeaturedODataSerializerProvider : DefaultODataSerializerProvider
    {
        private readonly FeaturedODataEntityTypeSerializer serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataSerializerProvider"/> class.
        /// </summary>
        public FeaturedODataSerializerProvider()
        {
            serializer = new FeaturedODataEntityTypeSerializer( this );
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
                return serializer.SerializationFeatures;
            }
        }

        /// <summary>
        /// Returns the serializer for the given type.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference">EDM type reference</see> to get a serializer for.</param>
        /// <returns>A <see cref="ODataEdmTypeSerializer"/> for the given <paramref name="edmType">EDM type</paramref>.</returns>
        public override ODataEdmTypeSerializer GetEdmTypeSerializer( IEdmTypeReference edmType ) =>
            edmType?.IsEntity() ?? false ? serializer : base.GetEdmTypeSerializer( edmType );
    }
}
