namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Edm;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents the serialization context for an OData feature.
    /// </summary>
    public class ODataSerializationFeatureContext
    {
        private readonly IEdmElement element;
        private readonly ODataSerializerContext serializerContext;
        private readonly ODataComplexTypeSerializer complexTypeSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializationFeatureContext"/> class.
        /// </summary>
        /// <param name="element">The <see cref="IEdmElement">element</see> represented by the instance.</param>
        /// <param name="serializerContext">The associated <see cref="ODataSerializerContext">serializer context</see>.</param>
        /// <param name="complexTypeSerializer">The serializer that can be used to serialize OData complex types.</param>
        public ODataSerializationFeatureContext( IEdmElement element, ODataSerializerContext serializerContext, ODataComplexTypeSerializer complexTypeSerializer )
        {
            Arg.NotNull( element, nameof( element ) );
            Arg.NotNull( serializerContext, nameof( serializerContext ) );
            Arg.NotNull( complexTypeSerializer, nameof( complexTypeSerializer ) );

            this.element = element;
            this.serializerContext = serializerContext;
            this.complexTypeSerializer = complexTypeSerializer;
        }

        /// <summary>
        /// Gets or sets the instance being serialized.
        /// </summary>
        /// <value>The instance being serialized.</value>
        public object Instance
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the element for the instance being serialized.
        /// </summary>
        /// <value>The <see cref="IEdmElement">element</see> for the instance being serialized.</value>
        public IEdmElement EdmElement
        {
            get
            {
                Contract.Ensures( element != null );
                return element;
            }
        }

        /// <summary>
        /// Gets the associated serializer context.
        /// </summary>
        /// <value>The associated <see cref="ODataSerializerContext">serializer context</see>.</value>
        public ODataSerializerContext SerializerContext
        {
            get
            {
                Contract.Ensures( serializerContext != null );
                return serializerContext;
            }
        }

        /// <summary>
        /// Gets the serializer that can be used to serialize OData complex types.
        /// </summary>
        /// <value>An <see cref="ODataComplexTypeSerializer"/> used to serialize complex types.</value>
        public ODataComplexTypeSerializer ComplexTypeSerializer
        {
            get
            {
                Contract.Ensures( complexTypeSerializer != null );
                return complexTypeSerializer;
            }
        }

        /// <summary>
        /// Gets the select or expand node the entry was created for.
        /// </summary>
        /// <value>The <see cref="SelectExpandNode">select or expand node</see> the entry was created for.</value>
        public SelectExpandNode SelectExpandNode
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the current entity instance context.
        /// </summary>
        /// <value>The current <see cref="EntityInstanceContext">entity instance context</see>.</value>
        public EntityInstanceContext EntityInstanceContext
        {
            get;
            set;
        }
    }
}
