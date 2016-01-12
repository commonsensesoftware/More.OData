namespace More.Web.OData.Formatter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.OData.Formatter.Serialization;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;

    /// <summary>
    /// Represents a <see cref="ODataComplexTypeSerializer">complex type serializer</see> that supports customizable serialization features.
    /// </summary>
    public class FeaturedODataComplexTypeSerializer : ODataComplexTypeSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataComplexTypeSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The underlying <see cref="ODataSerializerProvider">serializer provider</see>.</param>
        public FeaturedODataComplexTypeSerializer( ODataSerializerProvider serializerProvider )
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
        /// Creates and returns an OData complex type value using the specified object graph, tpye, and context.
        /// </summary>
        /// <param name="graph">The object graph to create a complex value for.</param>
        /// <param name="complexType">The type of complex type.</param>
        /// <param name="writeContext">The current <see cref="ODataSerializerContext">serializer context</see>.</param>
        /// <returns>The created <see cref="ODataComplexValue">complex value</see>.</returns>
        public override ODataComplexValue CreateODataComplexValue( object graph, IEdmComplexTypeReference complexType, ODataSerializerContext writeContext )
        {
            var complexValue = base.CreateODataComplexValue( graph, complexType, writeContext );
            //var context = new ODataEntrySerializationContext( complexValue, selectExpandNode, entityInstanceContext, complexSerializer );

            //foreach ( var feature in SerializationFeatures )
            //    feature.Apply( context );

            return complexValue;
        }
    }
}
