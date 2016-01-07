namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Defines the behavior of an annotation configuration.
    /// </summary>
    [ContractClass( typeof( IAnnotationConfigurationContract ) )]
    public interface IAnnotationConfiguration
    {
        /// <summary>
        /// Gets or sets the annotation namespace.
        /// </summary>
        /// <value>The annotation namespace.</value>
        [SuppressMessage( "Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Namespace", Justification = "Matches correct name. Will not cause cross-language issue." )]
        string Namespace
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the annotation name.
        /// </summary>
        /// <value>The annotation name.</value>
        string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Applies annotation configurations to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        void Apply( IEdmModel model );
    }
}
