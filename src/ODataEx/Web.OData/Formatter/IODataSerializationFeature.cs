namespace More.Web.OData.Formatter
{
    using Microsoft.OData.Core;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an OData serialization feature.
    /// </summary>
    [ContractClass( typeof( IODataSerializationFeatureContract ) )]
    public interface IODataSerializationFeature
    {
        /// <summary>
        /// Applies the serialization feature to the specified OData feed using the provided context.
        /// </summary>
        /// <param name="feed">The <see cref="ODataFeed"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        void Apply( ODataFeed feed, ODataSerializationFeatureContext context );

        /// <summary>
        /// Applies the serialization feature to the specified OData entry using the provided context.
        /// </summary>
        /// <param name="entry">The <see cref="ODataEntry"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        void Apply( ODataEntry entry, ODataSerializationFeatureContext context );

        /// <summary>
        /// Applies the serialization feature to the specified OData complex value using the provided context.
        /// </summary>
        /// <param name="complexValue">The <see cref="ODataComplexValue"/> to apply the serialization feature to.</param>
        /// <param name="context">The <see cref="ODataSerializationFeatureContext"/> used to apply feature-specific serialization.</param>
        void Apply( ODataComplexValue complexValue, ODataSerializationFeatureContext context );
    }
}
