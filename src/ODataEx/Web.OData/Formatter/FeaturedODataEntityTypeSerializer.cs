using System.Diagnostics.Contracts;
namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
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
        /// <param name="serializationFeatures">The <see cref="IList{T}">list</see> of <see cref="IODataSerializationFeature">serialization features</see> associated with the serializer.</param>
        public FeaturedODataEntityTypeSerializer( ODataSerializerProvider serializerProvider, IList<IODataSerializationFeature> serializationFeatures )
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
        /// Overrides the default behavior when an <see cref="ODataEntry">OData entry</see> is created.
        /// </summary>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode">select or expand node</see> to create the entry for.</param>
        /// <param name="entityInstanceContext">The current <see cref="EntityInstanceContext">entity instance context</see>.</param>
        /// <returns>A new <see cref="ODataEntry">OData entry</see> for the given node and context.</returns>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "Validated by a code contract." )]
        public override ODataEntry CreateEntry( SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext )
        {
            Contract.Assume( entityInstanceContext != null );

            var entry = base.CreateEntry( selectExpandNode, entityInstanceContext );
            var context = new ODataSerializationFeatureContext( entityInstanceContext.EntityType, entityInstanceContext.SerializerContext, ComplexTypeSerializer )
            {
                Instance = entityInstanceContext.TryGetEntityInstance(),
                EntityInstanceContext = entityInstanceContext,
                SelectExpandNode = selectExpandNode
            };

            foreach ( var feature in SerializationFeatures )
                feature.Apply( entry, context );

            return entry;
        }
    }
}
