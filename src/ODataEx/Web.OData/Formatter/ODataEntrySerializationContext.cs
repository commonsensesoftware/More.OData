namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using System.Diagnostics.Contracts;
    using System.Web.OData;
    using System.Web.OData.Formatter.Serialization;

    /// <summary>
    /// Represents the serialization context for an <see cref="ODataEntry">OData entry</see>.
    /// </summary>
    public class ODataEntrySerializationContext
    {
        private readonly ODataEntry entry;
        private readonly SelectExpandNode selectExpandNode;
        private readonly EntityInstanceContext entityInstanceContext;
        private readonly ODataComplexTypeSerializer complexTypeSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntrySerializationContext"/> class.
        /// </summary>
        /// <param name="entry">The current <see cref="ODataEntry">OData entry</see> being serialized.</param>
        /// <param name="selectExpandNode">The <see cref="SelectExpandNode">select or expand node</see> to the entry was created for.</param>
        /// <param name="entityInstanceContext">The current <see cref="EntityInstanceContext">entity instance context</see>.</param>
        /// <param name="complexTypeSerializer">The serializer that can be used to serialize OData complex types.</param>
        public ODataEntrySerializationContext( ODataEntry entry, SelectExpandNode selectExpandNode, EntityInstanceContext entityInstanceContext, ODataComplexTypeSerializer complexTypeSerializer )
        {
            Arg.NotNull( entry, nameof( entry ) );
            Arg.NotNull( selectExpandNode, nameof( selectExpandNode ) );
            Arg.NotNull( entityInstanceContext, nameof( entityInstanceContext ) );
            Arg.NotNull( complexTypeSerializer, nameof( complexTypeSerializer ) );

            this.entry = entry;
            this.selectExpandNode = selectExpandNode;
            this.entityInstanceContext = entityInstanceContext;
            this.complexTypeSerializer = complexTypeSerializer;
        }

        /// <summary>
        /// Gets the current entry being serialized.
        /// </summary>
        /// <value>The current <see cref="ODataEntry">OData entry</see> being serialized.</value>
        public ODataEntry Entry
        {
            get
            {
                Contract.Ensures( entry != null );
                return entry;
            }
        }

        /// <summary>
        /// Gets the select or expand node the entry was created for.
        /// </summary>
        /// <value>The <see cref="SelectExpandNode">select or expand node</see> the entry was created for.</value>
        public SelectExpandNode SelectExpandNode
        {
            get
            {
                Contract.Ensures( selectExpandNode != null );
                return selectExpandNode;
            }
        }

        /// <summary>
        /// Gets the current entity instance context.
        /// </summary>
        /// <value>The current <see cref="EntityInstanceContext">entity instance context</see>.</value>
        public EntityInstanceContext EntityInstanceContext
        {
            get
            {
                Contract.Ensures( entityInstanceContext != null );
                return entityInstanceContext;
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
    }
}
