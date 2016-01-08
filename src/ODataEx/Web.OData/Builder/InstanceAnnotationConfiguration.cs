namespace More.Web.OData.Builder
{
    using Microsoft.OData.Edm;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Web.OData.Builder;

    /// <summary>
    /// Represents the configuration for an instance annotation.
    /// </summary>
    [ContractClass( typeof( InstanceAnnotationConfigurationContract ) )]
    public abstract class InstanceAnnotationConfiguration : IAnnotationConfiguration
    {
        private readonly IEdmTypeConfiguration typeConfiguration;
        private bool hasNamespaceOverride;
        private string @namespace;
        private string name;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceAnnotationConfiguration"/> class.
        /// </summary>
        /// <param name="typeConfiguration">The associated <see cref="IEdmTypeConfiguration">type configuration</see>.</param>
        /// <param name="name">The name of the annotation.</param>
        protected InstanceAnnotationConfiguration( IEdmTypeConfiguration typeConfiguration, string name )
        {
            Arg.NotNull( typeConfiguration, nameof( typeConfiguration ) );
            Arg.NotNullOrEmpty( name, nameof( name ) );

            this.typeConfiguration = typeConfiguration;
            this.name = name;
        }

        /// <summary>
        /// Gets the name of the structural type to configure.
        /// </summary>
        /// <value>The qualified name of the structural type to configure.</value>
        public string StructuralTypeName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( Contract.Result<string>() ) );
                return typeConfiguration.FullName;
            }
        }

        /// <summary>
        /// Gets or sets the annotation namespace.
        /// </summary>
        /// <value>The annotation namespace.</value>
        public string Namespace
        {
            get
            {
                return hasNamespaceOverride ? @namespace : typeConfiguration.Namespace;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                @namespace = value;
                hasNamespaceOverride = true;
            }
        }

        /// <summary>
        /// Gets or sets the annotation name.
        /// </summary>
        /// <value>The annotation name.</value>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                name = value;
            }
        }

        /// <summary>
        /// Applies the configuration to the specified EDM model.
        /// </summary>
        /// <param name="model">The <see cref="IEdmModel">EDM model</see> to apply the configuration to.</param>
        [SuppressMessage( "Microsoft.Contracts", "CC1055", Justification = "Enforced by abstract contract" )]
        protected abstract void Apply( IEdmModel model );

        [SuppressMessage( "Microsoft.Contracts", "CC1055", Justification = "Enforced in protected implementation of Apply" )]
        void IAnnotationConfiguration.Apply( IEdmModel model ) => Apply( model );
    }
}
