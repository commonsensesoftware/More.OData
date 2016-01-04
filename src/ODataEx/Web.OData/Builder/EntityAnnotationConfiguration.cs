namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the configuration for an entity annotation.
    /// </summary>
    [ContractClass( typeof( EntityAnnotationConfigurationContract ) )]
    public abstract class EntityAnnotationConfiguration : IAnnotationConfiguration
    {
        private string entityTypeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityAnnotationConfiguration"/> class.
        /// </summary>
        /// <param name="entityTypeName">The name of the entity type to configure.</param>
        protected EntityAnnotationConfiguration( string entityTypeName )
        {
            Arg.NotNullOrEmpty( entityTypeName, nameof( entityTypeName ) );
            this.entityTypeName = entityTypeName;
        }

        /// <summary>
        /// Gets or sets the name of the entity type to configure.
        /// </summary>
        /// <value>The qualified name of the entity type to configure.</value>
        public string EntityTypeName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( entityTypeName ) );
                return entityTypeName;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                entityTypeName = value;
            }
        }

        /// <summary>
        /// Applies annotation configurations to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Contracts", "CC1055", Justification = "Enforced by abstract contract" )]
        public abstract void Apply( IEdmModel model );
    }
}
