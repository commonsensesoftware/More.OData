namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents a <see cref="ODataComplexTypeSerializer">complex type serializer</see> that supports customizable serialization features.
    /// </summary>
    public class FeaturedODataComplexTypeSerializer : ODataComplexTypeSerializer
    {
        private readonly ODataComplexTypeSerializer complexSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataComplexTypeSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The underlying <see cref="ODataSerializerProvider">serializer provider</see>.</param>
        /// <param name="serializationFeatures">The <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see> associated with the serializer.</param>
        public FeaturedODataComplexTypeSerializer( ODataSerializerProvider serializerProvider, IList<IODataSerializationFeature> serializationFeatures )
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
        /// Creates and returns an OData complex type value using the specified object graph, tpye, and context.
        /// </summary>
        /// <param name="graph">The object graph to create a complex value for.</param>
        /// <param name="complexType">The type of complex type.</param>
        /// <param name="writeContext">The current <see cref="ODataSerializerContext">serializer context</see>.</param>
        /// <returns>The created <see cref="ODataComplexValue">complex value</see>.</returns>
        public override ODataComplexValue CreateODataComplexValue( object graph, IEdmComplexTypeReference complexType, ODataSerializerContext writeContext )
        {
            var complexValue = base.CreateODataComplexValue( graph, complexType, writeContext );
            var context = new ODataSerializationFeatureContext( complexType, writeContext, ComplexTypeSerializer ) { Instance = graph };

            foreach ( var feature in SerializationFeatures )
                feature.Apply( complexValue, context );

            return complexValue;
        }
    }
}
