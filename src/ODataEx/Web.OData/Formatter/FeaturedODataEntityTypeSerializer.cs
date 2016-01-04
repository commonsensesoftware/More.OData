namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using System.Collections.Generic;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents an <see cref="ODataEntityTypeSerializer">entity serializer</see> that supports customizable serialization features.
    /// </summary>
    public class FeaturedODataEntityTypeSerializer : ODataEntityTypeSerializer
    {
        private readonly ODataComplexTypeSerializer complexSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeaturedODataEntityTypeSerializer"/> class.
        /// </summary>
        /// <param name="serializerProvider">The underlying <see cref="ODataSerializerProvider">serializer provider</see>.</param>
        public FeaturedODataEntityTypeSerializer( ODataSerializerProvider serializerProvider )
            : base( serializerProvider )
        {
            complexSerializer = new ODataComplexTypeSerializer( serializerProvider );
        }

        /// <summary>
        /// Gets a list of serialization features for the serializer.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see>.</value>
        /// <remarks>The default list of serialization features include the <see cref="InstanceAnnotationSerializationFeature"/>
        /// and the <see cref="MediaResourceSerializationFeature"/>.</remarks>
        public IList<IODataSerializationFeature> SerializationFeatures
        {
            get;
        } = new List<IODataSerializationFeature>();

        /// <summary>
        /// Overrides the default behavior when an <see cref="ODataEntry">OData entry</see> is created.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode">select or expand node</see> to create the entry for.</param>
        /// <param name="entityInstanceContext">The current <see cref="EntityInstanceContext">entity instance context</see>.</param>
        /// <returns>A new <see cref="ODataEntry">OData entry</see> for the given node and context.</returns>
        public override ODataEntry CreateEntry( SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext )
        {
            var entry = base.CreateEntry( selectExpandNode, entityInstanceContext );
            var context = new ODataEntrySerializationContext( entry, selectExpandNode, entityInstanceContext, complexSerializer );

            foreach ( var feature in SerializationFeatures )
                feature.Apply( context );
            
            return entry;
        }
    }
}
