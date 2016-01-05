namespace More.Web.OData.Formatter
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an OData serialization feature.
    /// </summary>
    [ContractClass( typeof( IODataSerializationFeatureContract ) )]
    public interface IODataSerializationFeature
    {
        /// <summary>
        /// Applies the serialization feature using the provided context.
        /// </summary>
        /// <param name="context">The <see cref="ODataEntrySerializationContext"/> used to apply feature-specific serialization.</param>
        void Apply( ODataEntrySerializationContext context );
    }
}
