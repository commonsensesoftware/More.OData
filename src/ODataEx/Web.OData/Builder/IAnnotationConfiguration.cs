namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an annotation configuration.
    /// </summary>
    [ContractClass( typeof( IAnnotationConfigurationContract ) )]
    public interface IAnnotationConfiguration
    {
        /// <summary>
        /// Applies annotation configurations to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        void Apply( IEdmModel model );
    }
}
